## Run

### Prereqs
- Docker (lol)
- yum install samba-client samba-common cifs-utils
- ssh root@vt-dipstube01

```bash
mount -t cifs \\\\p-fs01\\DIPS /mnt/dips -o username=roh,password='verdensbestepassord',domain=dips-ad,vers=2.0

ln -s "/mnt/dips/Linjeorganisasjon/Utviklingsavdelingen/Team/Health Angels/Interessanttimer" Interesting

ln -s "/mnt/dips/Linjeorganisasjon/Utviklingsavdelingen/Enhet SAO/Team/Team Optimus/OptimusInteresting" OptimusInteresting

ln -s "/mnt/dips/Linjeorganisasjon/Utviklingsavdelingen/Arkitektur og teknologi/External Talks" External

ln -s "/mnt/dips/Linjeorganisasjon/Utviklingsavdelingen/Utviklerforum" Utviklerforum

ln -s "/mnt/dips/Linjeorganisasjon/Utviklingsavdelingen/Sprintdemo" Sprintdemo

ln -s "/mnt/dips/Linjeorganisasjon/Utviklingsavdelingen/Video utviklerforum, sprintdemo, møter og kurs/Møter og workshops/DDD 2019/opptak sesjoner" DevDays2019
```

