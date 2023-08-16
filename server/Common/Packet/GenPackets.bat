START /WAIT ../../PacketGenerator/bin/Debug/PacketGenerator.exe ../../PacketGenerator/bin/Debug/PDL.xml
XCOPY /Y GenPackets.cs "../../DummyClient/Packet"
XCOPY /Y GenPackets.cs "../../server/server/Packet"
XCOPY /Y ClientPacketManager.cs "../../DummyClient/Packet"
XCOPY /Y ServerPacketManager.cs "../../server/server/Packet"
if %ERRORLEVEL% == 0 (
    echo "복사 완료!"
) else (
    echo "복사 실패. 오류 코드: %ERRORLEVEL%"
)

pause