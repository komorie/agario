START ../../PacketGenerator/bin/Debug/PacketGenerator.exe ../../PacketGenerator/bin/PDL.xml
XCOPY /Y "GenPackets.cs" "../../DummyClient/Packet"
XCOPY /Y "GenPackets.cs" "../../server/server/Packet"
if %ERRORLEVEL% == 0 (
    echo "���� �Ϸ�!"
) else (
    echo "���� ����. ���� �ڵ�: %ERRORLEVEL%"
)

pause