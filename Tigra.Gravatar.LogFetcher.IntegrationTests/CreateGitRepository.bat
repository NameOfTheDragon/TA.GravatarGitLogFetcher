REM Creates a new, empty Git repository, imports some sample content and commits it as Tim Long
REM then adds a new text file and commits it as Darth Vader
REM Dependencies:
REM  - Git must be installed
REM  - Robocopy must be installed

REM Using RoboCopy /MIR will wipe out any pre-existing content including Git repository (caution!)
RoboCopy .\TestContent .\GitIntegrationTestRepository /MIR

REM Initialize the Git repository
cd GitIntegrationTestRepository
git.exe init

REM Import sample content, commit as Tim Long
git.exe add *.*
git.exe commit -m "Initial import" --author="Tim Long <Tim@tigranetworks.co.uk>"

REM Create a new file, commit as Darth Vader
echo "A new file!" >NewFile.txt
git.exe add NewFile.txt
git.exe commit -m "Added a new file, NewFile.txt" --author="Darth Vader <Darth@deathstar.space>"
