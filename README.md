# FileShoal
This application can be used to copy files to a number of different systems via ssh/scp.

The transfer is handled by the WinSCP Library https://winscp.net/eng/docs/library

Authentication is done via ssh-keys. These can be generated with PuTTYgen. After transferring the private key to each target machine, you will have to add the host key of the target machine to the machine configuration file.
