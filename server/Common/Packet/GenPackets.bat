START /WAIT ../../PacketGenerator/bin/Debug/PacketGenerator.exe ../../PacketGenerator/bin/Debug/PDL.xml
XCOPY /Y GenPackets.cs "../../DummyClient/Packet"
XCOPY /Y GenPackets.cs "../../server/server/Packet"
XCOPY /Y ClientPacketManager.cs "../../DummyClient/Packet"
XCOPY /Y ServerPacketManager.cs "../../server/server/Packet"
if %ERRORLEVEL% == 0 (
    echo "���� �Ϸ�!"
) else (
    echo "���� ����. ���� �ڵ�: %ERRORLEVEL%"
)

pause