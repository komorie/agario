using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace PacketGenerator
{
    internal class Program
    {

        private static string genPackets = "";

        static void Main(string[] args) //XML 파일로부터 패킷 클래스들이 모인 cs파일을 생성하는 과정.
        {
            XmlReaderSettings settings = new XmlReaderSettings()
            {
                IgnoreComments = true,
                IgnoreWhitespace = true
            };

            using (XmlReader r = XmlReader.Create("PDL.xml", settings)) //using으로 자동 Dispose (파일 리더 등은 가비지 컬렉터가 관리하지 않으므로 명시적으로 해줘야함)
            {
                r.MoveToContent(); //헤더 건너뛰기

                while (r.Read()) //한줄씩 읽기
                {
                    if(r.Depth == 1 && r.NodeType == XmlNodeType.Element)  //깊이 1일때 (<packet>) 패킷을 파싱한다
                    {
                        ParsePacket(r);
                    }
                }

                File.WriteAllText("GenPackets.cs", genPackets); //패킷 틀 문자열의 빈칸을 채워서, cs 파일로 생성. 이를 파일 끝까지 반복

            }


        }

        public static void ParsePacket(XmlReader r)
        {
            if (r.NodeType == XmlNodeType.EndElement) return; //닫는 태그면 스킵

            if (r.Name.ToLower() != "packet") return; //패킷 아니면 스킵

            string packetName = r.Name; //패킷 이름

            if(string.IsNullOrEmpty(packetName)) //패킷 이름 안비었는지 체크
            {
                Console.WriteLine("Packet Name is Null");
                return;
            }

            Tuple<string, string, string> t = ParseMembers(r); //패킷 변수, Read함수, Write함수 포맷 만들어서 리턴
            genPackets += string.Format(PacketFormat.packetFormat, packetName, t.Item1, t.Item2, t.Item3); //세마리 모아서 엑조디아 패킷 만들기
        }

        public static Tuple<string, string, string> ParseMembers(XmlReader r)
        {
            string packetName = r["name"]; // <packet name="PlayerInfoReq"> 

            string memberCode = ""; //멤버변수 포맷
            string readCode = ""; //Read함수 포맷
            string writeCode = ""; //Write함수 포맷

            int depth = r.Depth + 1;

            while (r.Read())
            {
                if (r.Depth != depth) break;//깊이가 2인 패킷 내부 요소들만 읽어오기

                string memberName = r["name"]; //<long name = "playerId"/> 의 playerId 부분이 비었는지 체크

                if (string.IsNullOrEmpty(memberName))
                {
                    Console.WriteLine("Member Without Name");
                    return null;
                }

                if (string.IsNullOrEmpty(memberCode) == false) //내용이 있으면 다음 줄로 넘어가기
                {
                    memberCode += Environment.NewLine; //엔터
                }

                if (string.IsNullOrEmpty(readCode) == false) //내용이 있으면 다음 줄로 넘어가기
                {
                    readCode += Environment.NewLine; //엔터
                }

                if (string.IsNullOrEmpty(writeCode) == false) //내용이 있으면 다음 줄로 넘어가기
                {
                    writeCode += Environment.NewLine; //엔터
                }

                string memberType = r.Name.ToLower(); //<long name = "playerId"/>의 long 부분
                switch (memberType)
                {
                    case "bool":
                    case "byte":
                    case "short":
                    case "ushort":
                    case "int":
                    case "long":
                    case "float":
                    case "double":
                        memberCode += string.Format(PacketFormat.memberFormat, memberType, memberName); //public memberType memberName;

                        readCode += string.Format(PacketFormat.readFormat, memberName, ToMemberType(memberType), memberType);
                        //this.memberName = BitConverter.ToMemberType(memberType)(seg.Slice(count, seg.Length - count));
                        //count += sizeof(memberType);

                        writeCode += string.Format(PacketFormat.writeFormat, memberName, memberType);
                        //success &= BitConverter.TryWriteBytes(seg.Slice(count, seg.Length - count), this.{0}); //샌드 버퍼의 부분에 값 작성
                        //count += sizeof({1});
                        break;
                    case "string":
                        memberCode += string.Format(PacketFormat.memberFormat, memberType, memberName); //public memberType memberName;

                        readCode += string.Format(PacketFormat.readStringFormat, memberName);
                        //string 가져오려면, 먼저 사이즈 가져오기
                        //ushort {0}Len = BitConverter.ToUInt16(seg.Slice(count, seg.Length - count));
                        //count += sizeof(ushort);

                        //가져온 사이즈만큼, 문자열 가져오기
                        //this.{0} = Encoding.Unicode.GetString(seg.Slice(count, {0}Len));
                        //count += {0}Len;


                        writeCode += string.Format(PacketFormat.writeStringFormat, memberName);
                        //문자열의 길이만큼, 바이트로 변환 후 seg에 작성을 한번에! 대신 스트링 길이를 앞에 작성해야 하므로 위치는 ushort 크기만큼 뒤로 보냄
                        //ushort {0}Len = (ushort)Encoding.Unicode.GetBytes(this.{0}, 0, this.{0}.Length, seg.Array, seg.Offset + count + sizeof(ushort));

                        //문자열의 크기 seg에 작성
                        //success &= BitConverter.TryWriteBytes(seg.Slice(count, seg.Count - count), {0}Len);
                        //count += sizeof(ushort);
                        //count += {0}Len;
                        break;
                    case "list":
                        break;
                    default:
                        break;
                }
            }
            memberCode = memberCode.Replace("\n", "\n\t"); //줄맞춤
            readCode = readCode.Replace("\n", "\n\t\t"); 
            writeCode = writeCode.Replace("\n", "\n\t\t");

            return new Tuple<string, string, string>(memberCode, readCode, writeCode);
        }

        public static string ToMemberType(string memberType)
        {
            switch (memberType)
            {
                case "bool":
                    return "ToBoolean";
                case "short":
                    return "ToInt16";
                case "ushort":
                    return "ToUInt16";
                case "int":
                    return "ToInt32";
                case "long":
                    return "ToInt64";
                case "float":
                    return "ToSingle";
                case "double":
                    return "ToDouble";
                default:
                    return "";
            }
        }
    }
}