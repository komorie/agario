using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace PacketGenerator
{
    internal class Program
    {
        private static int packetId; //Enum용 패킷 번호
        private static string packetEnums; //패킷 구분할, 이름 모아놓은 Enum

        private static string clientRegister; //패킷 종류에 따라 생성하는 함수 등록  (클라쪽)
        private static string serverRegister; //패킷 종류에 따라 생성하는 함수 등록 (서버쪽)


        private static string genPackets; //패킷 XML 파싱해서 생성한 본체들

        static void Main(string[] args) //XML 파일로부터 패킷 클래스들이 모인 cs파일을 생성하는 과정.
        {

            string pdlPath = "PDL.xml";

            if(args.Length >= 1)
            {
                pdlPath = args[0]; //실행 시 넣은 경로 인자
            }

            XmlReaderSettings settings = new XmlReaderSettings()
            {
                IgnoreComments = true,
                IgnoreWhitespace = true
            };

            using (XmlReader r = XmlReader.Create(pdlPath, settings)) //using으로 자동 Dispose (파일 리더 등은 가비지 컬렉터가 관리하지 않으므로 명시적으로 해줘야함)
            {
                r.MoveToContent(); //헤더 건너뛰기

                while (r.Read()) //한줄씩 읽기
                {
                    if(r.Depth == 1 && r.NodeType == XmlNodeType.Element)  //깊이 1일때
                    {
                        ParsePacket(r); //(<packet>) 태그의 내용물을 파싱한다
                    }
                }

                string fileText = string.Format(OurPacketFormat.fileFormat, packetEnums, genPackets);
                File.WriteAllText("GenPackets.cs", fileText); //패킷 틀 문자열의 빈칸을 채워서, cs 파일로 생성. 이를 파일 끝까지 반복

                string clientManagerText = string.Format(OurPacketFormat.managerFormat, clientRegister);
                File.WriteAllText("ClientPacketManager.cs", clientManagerText); //패킷 매니저 틀 문자열의 빈칸을 채워서, cs 파일로 생성. (클라가 받을 패킷만)

                string serverManagerText = string.Format(OurPacketFormat.managerFormat, serverRegister);
                File.WriteAllText("ServerPacketManager.cs", serverManagerText); //패킷 매니저 틀 문자열의 빈칸을 채워서, cs 파일로 생성. (서버가 받을 패킷만)
            }


        }

        public static void ParsePacket(XmlReader r)
        {
            if (r.NodeType == XmlNodeType.EndElement) return; //닫는 태그면 스킵

            if (r.Name.ToLower() != "packet") return; //패킷 아니면 스킵

            string packetName = r["name"]; //패킷 이름

            if(string.IsNullOrEmpty(packetName)) //패킷 이름 안비었는지 체크
            {
                Console.WriteLine("Packet Name is Null");
                return;
            }

            Tuple<string, string, string> t = ParseMembers(r); //패킷 변수, Read함수, Write함수 포맷 만들어서 리턴
            genPackets += string.Format(OurPacketFormat.packetFormat, packetName, t.Item1, t.Item2, t.Item3); //세마리 모아서 엑조디아 패킷 만들기
            packetEnums += string.Format(OurPacketFormat.packetEnumFormat, packetName, ++packetId) + Environment.NewLine + "\t"; //Enum에 패킷이름 집어넣기
            
            //패킷 매니저 포맷에 Enum에 패킷이름 집어넣고, newline, tap 2번 넣기 (패킷 이름에 따라 어느쪽에 들어갈 지 구분)
            //굳이 나누는 이유는, packet은 둘 다 알아야 하지만 packethandler의 경우 받는 쪽만 구현해도 되기 때문이다.
            if(packetName.StartsWith("S_") || packetName.StartsWith("s_")) //서버가 보내는 패킷임
            {
                clientRegister += (string.Format(OurPacketFormat.managerRegisterFormat, packetName) + Environment.NewLine + "\t\t"); //그럼 클라 쪽 매니저에 추가
            }
            else //클라가 보내는 패킷임
            {
                serverRegister += (string.Format(OurPacketFormat.managerRegisterFormat, packetName) + Environment.NewLine + "\t\t");
            }
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
                    case "byte":
                    case "sbyte":
                        memberCode += string.Format(OurPacketFormat.memberFormat, memberType, memberName); //public memberType memberName;
                        readCode += string.Format(OurPacketFormat.readByteFormat, memberName, memberType);
                        writeCode += string.Format(OurPacketFormat.writeByteFormat, memberName, memberType);
                        break;
                    case "bool":
                    case "short":
                    case "ushort":
                    case "int":
                    case "long":
                    case "float":
                    case "double":
                        memberCode += string.Format(OurPacketFormat.memberFormat, memberType, memberName); //public memberType memberName;

                        readCode += string.Format(OurPacketFormat.readFormat, memberName, ToMemberType(memberType), memberType);

                        //this.memberName = BitConverter.ToMemberType(memberType)(seg.Slice(count, seg.Length - count));
                        //count += sizeof(memberType);

                        writeCode += string.Format(OurPacketFormat.writeFormat, memberName, memberType);
                        //success &= BitConverter.TryWriteBytes(seg.Slice(count, seg.Length - count), this.{0}); //샌드 버퍼의 부분에 값 작성
                        //count += sizeof({1});
                        break;
                    case "string":
                        memberCode += string.Format(OurPacketFormat.memberFormat, memberType, memberName); //public memberType memberName;

                        readCode += string.Format(OurPacketFormat.readStringFormat, memberName);
                        //string 가져오려면, 먼저 사이즈 가져오기
                        //ushort {0}Len = BitConverter.ToUInt16(seg.Slice(count, seg.Length - count));
                        //count += sizeof(ushort);

                        //가져온 사이즈만큼, 문자열 가져오기
                        //this.{0} = Encoding.Unicode.GetString(seg.Slice(count, {0}Len));
                        //count += {0}Len;


                        writeCode += string.Format(OurPacketFormat.writeStringFormat, memberName);
                        //문자열의 길이만큼, 바이트로 변환 후 seg에 작성을 한번에! 대신 스트링 길이를 앞에 작성해야 하므로 위치는 ushort 크기만큼 뒤로 보냄
                        //ushort {0}Len = (ushort)Encoding.Unicode.GetBytes(this.{0}, 0, this.{0}.Length, seg.Array, seg.Offset + count + sizeof(ushort));

                        //문자열의 크기 seg에 작성
                        //success &= BitConverter.TryWriteBytes(seg.Slice(count, seg.Count - count), {0}Len);
                        //count += sizeof(ushort);
                        //count += {0}Len;
                        break;
                    case "list":
                        Tuple<string, string, string> t = ParseList(r);
                        memberCode += t.Item1;
                        readCode += t.Item2;    
                        writeCode += t.Item3;   
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

        public static Tuple<string, string, string> ParseList(XmlReader r)
        {
            string listName = r["name"];
            if (string.IsNullOrEmpty(listName))
            {
                Console.WriteLine("List without name");
                return null;
            }

            Tuple<string, string, string> t = ParseMembers(r); //list 구조체의 내부 원소들은 ParseMember 재사용해서 가져오기

            string memberCode = string.Format(OurPacketFormat.memberListFormat,
                FirstCharToUpper(listName),
                FirstCharToLower(listName),
                t.Item1,
                t.Item2,
                t.Item3);

            string readCode = string.Format(OurPacketFormat.readListFormat,
                FirstCharToUpper(listName),
                FirstCharToLower(listName));

            string writeCode = string.Format(OurPacketFormat.writeListFormat,
                FirstCharToUpper(listName),
                FirstCharToLower(listName));

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

        public static string FirstCharToUpper(string input) //첫글자 대문자
        {
            if (string.IsNullOrEmpty(input))
                return "";
            return input[0].ToString().ToUpper() + input.Substring(1);
        }

        public static string FirstCharToLower(string input) //첫글자 소문자
        {
            if (string.IsNullOrEmpty(input))
                return "";
            return input[0].ToString().ToLower() + input.Substring(1);
        }
    }
}