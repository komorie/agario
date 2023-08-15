START ../../PacketGenerator/bin/Debug/PacketGenerator.exe ../../PacketGenerator/bin/PDL.xml
XCOPY /Y "GenPackets.cs" "../../DummyClient/Packet"
XCOPY /Y "GenPackets.cs" "../../server/server/Packet"
if %ERRORLEVEL% == 0 (
    echo "복사 완료!"
) else (
    echo "복사 실패. 오류 코드: %ERRORLEVEL%"
)

pause