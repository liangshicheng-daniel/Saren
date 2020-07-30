import mysql.connector
import pytz
from datetime import datetime, date, timezone, timedelta

mydb = mysql.connector.connect(
  host="sarenbot.csmgbnunb1g2.us-east-2.rds.amazonaws.com",
  user="saren",
  passwd="testing123456",
  database="Saren_Bot"
)

tz = pytz.timezone('Asia/Shanghai')
today_start_str = datetime.now(tz).strftime("%Y-%m-%d")+' 05:00:00.000000+08:00'
today_start = datetime.fromisoformat(today_start_str)
now = datetime.now(tz)
#now = datetime.fromisoformat('2020-05-02 04:00:00.000000+08:00')
if now < today_start:
    today_start -= timedelta(days = 1)
today_start_str = today_start.strftime("%Y-%m-%d")+'T05:00:00'
print(today_start_str)
# today_start = datetime.fromisoformat(today_start_str).replace(tzinfo=tz)

