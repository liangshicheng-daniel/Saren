from nonebot import on_command, CommandSession
import mysql.connector
import numbers
import pytz
import re
from datetime import datetime, date, timezone, timedelta


@on_command('new_run', aliases=('申请出刀', '出刀'), only_to_me=False)
async def new_run(session: CommandSession):
    if session.ctx['message_type'] == 'group':
        x=re.search(r".*\s*\[CQ\:at,qq\=(\d+)\]", session.ctx['raw_message'])      
        user_id=session.ctx['user_id']
        if x != None:
            if not_supervisor(user_id):
                await session.send('代人操作仅限指挥使用')
                return
            user_id = x.group(1)
        response = await start_new_run(user_id)
        await session.send(response)

@on_command('test', aliases=('测试','test'), only_to_me=False)
async def test(session: CommandSession):
    if session.ctx['message_type'] == 'group':
        print(session.ctx['raw_message'])
        print(session.current_arg_text)


@on_command('end_run', aliases=('完成出刀','整刀', '报刀'), only_to_me=False)
async def end_run(session: CommandSession):
    if session.ctx['message_type'] == 'group':
        x=re.search(r".*\s*\[CQ\:at,qq\=(\d+)\]", session.ctx['raw_message'])      
        user_id=session.ctx['user_id']
        player_id = None
        if x != None:
            if not_supervisor(user_id):
                await session.send('代人操作仅限指挥使用')
                return
            player_id = user_id
            user_id = x.group(1)
        temp = session
        if session.current_arg_text is None or session.current_arg_text == '':
            response = await end_run(user_id, player_id, '', '完成出刀', temp)    
        else:  
            response = await end_run(user_id, player_id, session.current_arg_text, '完成出刀', temp)     
        await session.send(response)


@on_command('end_run_fail', aliases=('强行下树', '下树'), only_to_me=False)
async def end_run_fail(session: CommandSession):
    if session.ctx['message_type'] == 'group':
        x=re.search(r".*\s*\[CQ\:at,qq\=(\d+)\]", session.ctx['raw_message'])      
        user_id=session.ctx['user_id']
        player_id = None
        if x != None:
            if not_supervisor(user_id):
                await session.send('代人操作仅限指挥使用')
                return
            player_id = user_id
            user_id = x.group(1)
        temp = session
        if session.current_arg_text is None or session.current_arg_text == '':
            response = await end_run(user_id, player_id, '', '强行下树', temp)    
        else:  
            response = await end_run(user_id, player_id, session.current_arg_text, '强行下树', temp)       
        await session.send(response)


@on_command('hangon', aliases=('挂树', '上树'), only_to_me=False)
async def hangon(session: CommandSession):
    if session.ctx['message_type'] == 'group':
        x=re.search(r".*\s*\[CQ\:at,qq\=(\d+)\]", session.ctx['raw_message'])      
        user_id=session.ctx['user_id']
        if x != None:
            if not_supervisor(user_id):
                await session.send('代人操作仅限指挥使用')
                return
            user_id = x.group(1)
        nickname = get_nickname(user_id)
        con = database_connection()
        cursor = con.cursor()
        query = "SELECT * FROM Battles where status = 'OnTree' and member_id = {}".format(user_id)
        cursor.execute(query)
        if cursor.fetchone() is not None:
            await session.send('弟弟您已经在树上了，请不要重复挂树')
        else:
            battle_id = find_battle_record(user_id)
            if battle_id is None:
                if check_total_runs(user_id) >= 3:
                    await session.send('您今日已出满三刀，请不要调戏Saren，不然打飞你哦')
                else:
                    confirmation = session.get('confirmation', prompt='您还没有出刀，请确认您是否已挂树')
                    if confirmation == '确认':
                        query = "SELECT event_id, current_boss_id, current_cycle from Parameters where Parameters.status = 'Active'"
                        cursor.execute(query)
                        result = cursor.fetchone()
                        query = "INSERT INTO Battles (member_id, event_id, boss_id, cycle_number, status) VALUES (%s, %s, %s, %s, %s)"
                        val = (user_id, result[0], result[1], result[2], 'OnTree')
                        cursor.execute(query, val)
                        con.commit()
                        await session.send('{} 已挂树'.format(nickname))
                    else:
                        await session.send('取消挂树')
            else:
                query = "UPDATE Battles SET status = 'OnTree' where battle_id = {}".format(battle_id[0])
                cursor.execute(query)
                con.commit()
                await session.send('{} 已挂树'.format(nickname))


@on_command('check_tree', aliases=('查树',), only_to_me=False)
async def check_tree(session: CommandSession):
    if session.ctx['message_type'] == 'group':
        user_id=session.ctx['user_id']
        con = database_connection()
        cursor = con.cursor()
        query = "SELECT Member.nickname from Member inner join Battles on Member.member_id = Battles.member_id where Battles.record_time > '2020-04-30T05:00:00' and Battles.status = 'OnTree'"
        cursor.execute(query)
        list = cursor.fetchall()
        if len(list) == 0:
            await session.send('目前无人上树')
        else:
            response = '树上挂着的有：\n'
            for member in list:
                response += '    {}\n'.format(member[0])   
            await session.send(response)


@on_command('last_run', aliases=('尾刀',), only_to_me=False)
async def last_run(session: CommandSession):    
    if session.ctx['message_type'] == 'group':
        x=re.search(r".*\s*\[CQ\:at,qq\=(\d+)\]", session.ctx['raw_message'])      
        user_id=session.ctx['user_id']
        player_id = None
        if x != None:
            if not_supervisor(user_id):
                await session.send('代人操作仅限指挥使用')
                return
            player_id = user_id
            user_id = x.group(1)
        battle_id = find_battle_record(user_id)      
        if battle_id is None:
            if check_total_runs(user_id) >= 3:
                await session.send('您今日已出满三刀，请不要调戏Saren，不然打飞你哦')
            else:
                confirmation = session.get('confirmation', prompt='您还没有出刀，请确认您已击杀Boss')
                if confirmation.split()[0] == '确认':
                    con = database_connection()
                    cursor = con.cursor()   
                    result = update_parameters()
                    if (player_id is None):
                        query = "INSERT INTO Battles (member_id, event_id, boss_id, cycle_number, damage, status) VALUES (%s, %s, %s, %s, %s, %s)"
                        val = (user_id, result[0], result[1], result[2], result[3], 'Last_Hit')
                    else:
                        query = "INSERT INTO Battles (member_id, player_id, event_id, boss_id, cycle_number, damage, status) VALUES (%s, %s, %s, %s, %s, %s, %s)"
                        val = (user_id, player_id, result[0], result[1], result[2], result[3], 'Last_Hit')
                    cursor.execute(query, val)
                    con.commit()
                    cut_tree()
                    await session.send(final_message(user_id, result[6]))
                else:
                    await session.send('本次尾刀没有被记录')
        else:
            confirmation1 = session.get('confirmation', prompt='请确认Boss已被击杀')         
            if confirmation1.split()[0] == '确认':
                con = database_connection()
                cursor = con.cursor()
                result = update_parameters()  
                if player_id is None:
                    query = "UPDATE Battles SET damage = {}, status = 'Last_Hit' where battle_id = {}".format(result[3], battle_id[0])
                else:
                    query = "UPDATE Battles SET damage = {}, status = 'Last_Hit', player_id = {} where battle_id = {}".format(result[3], player_id, battle_id[0])
                cursor.execute(query)
                con.commit()
                cut_tree()
                await session.send(final_message(user_id, result[6]))
            else:
                await session.send('本次尾刀没有被记录')


@on_command('police', aliases=('逃兵报告', '出刀情况', '出刀出警', '出警'), only_to_me=False)
async def police(session: CommandSession):    
    if session.ctx['message_type'] == 'group':
        user_id=session.ctx['user_id']
        con = database_connection()
        cursor = con.cursor()
        query = "SELECT count(battle_id), Member.nickname from Member left join Battles on Member.member_id = Battles.member_id and Battles.record_time > '{}' and Battles.status = 'Finished' where Member.role != 'inactive' group by Member.nickname order by count(battle_id)".format(get_refresh_time())
        cursor.execute(query)
        list = cursor.fetchall()
        query = "SELECT count(battle_id) from Battles where Battles.record_time > '{}' and Battles.status = 'Finished'".format(get_refresh_time())
        cursor.execute(query)
        battleCount = cursor.fetchone()
        if battleCount[0] == 90:
            await session.send('今日所有团员皆已出刀，工会战警察 Saren 下班！')
        else:
            response = '今日尚有{}刀未出：\n'.format(90-battleCount[0])
            count = 0
            for member in list:
                if (member[0]<3):
                    response += '{}, {}刀；  '.format(member[1], 3-member[0])
                    if count == 10:
                        await session.send(response)
                        count = 0
                        response = ''
                    count += 1
            await session.send(response)
        #SELECT count(battle_id), Member.member_id from Member left join Battles on Member.member_id = Battles.member_id and Battles.record_time > '2020-04-30T05:00:00' and Battles.status = 'Finished' group by member_id order by count(battle_id)


@on_command('help', aliases=('指令集', '帮助'))
async def help(session: CommandSession):
    response = '目前可以使用的指令有：\n    申请出刀\n    完成出刀 (伤害值)\n    尾刀\n    挂树\n    查树\n    强行下树 (伤害值)\n    出警\n    查刀\n    作业\n    sl\n    查sl\n    回档 【管理限定】\n    血量重设 (正确血量) 【管理限定】\n如果Saren需要确认，请回复 确认'
    await session.send(response)


@on_command('reset_last_run', aliases=('回档',), only_to_me=False)
async def reset_last_run(session: CommandSession):
    if session.ctx['message_type'] == 'group':
        user_id=session.ctx['user_id']
        if not_supervisor(user_id):
            await session.send('回档仅限指挥使用')
            return
        else:
            con = database_connection()
            cursor = con.cursor()
            query = "SELECT Battles.battle_id, Battles.cycle_number, Battles.damage, Battles.status, Battles.record_time, Member.nickname, Boss.boss_name, Battles.event_id FROM Battles inner join Member on Battles.member_id = Member.member_id inner join Boss on Battles.boss_id = Boss.boss_id where status = 'OnTree' or status = 'OffTree' or status = 'Running' or status = 'Finished' or status = 'Last_Hit' order by battle_id desc LIMIT 1"
            cursor.execute(query)
            record = cursor.fetchone()
            if record[3] == 'Running':
                prompt = '请确认是否回档以下记录：\n    {} 申请对第{}轮的Boss: {}出刀\n    时间：{}'.format(record[5], record[1], record[6], record[4])
                confirmation = session.get('confirmation', prompt=prompt)
                if confirmation == '确认':
                    query = "UPDATE Battles SET status = 'Abandoned' where battle_id = {}".format(record[0])
                    cursor.execute(query)
                    con.commit()  
                    await session.send('出刀申请已被取消')
                    return
                else:
                    await session.send('回档尝试被取消')
                    return
            elif record[3] == 'OnTree':
                prompt = '请确认是否回档以下记录：\n    {} 申请挂树\n    时间：{}'.format(record[5], record[4])
                confirmation = session.get('confirmation', prompt=prompt)
                if confirmation == '确认':
                    query = "UPDATE Battles SET status = 'Abandoned' where battle_id = {}".format(record[0])
                    cursor.execute(query)
                    con.commit()  
                    await session.send('挂树申请已被取消')
                    return
                else:
                    await session.send('回档尝试被取消')
                    return
            elif record[3] == 'Finished' or record[3] == 'OffTree':
                prompt = '请确认是否回档以下记录：\n    {} 完成对第{}轮的Boss: {}出刀，造成伤害{}\n    时间：{}'.format(record[5], record[1], record[6], record[2], record[4])
                confirmation = session.get('confirmation', prompt=prompt)
                if confirmation == '确认':
                    query = "UPDATE Battles SET status = 'Abandoned' where battle_id = {}".format(record[0])
                    cursor.execute(query)
                    con.commit()  
                    query = 'SELECT current_boss_health from Parameters where event_id = {}'.format(record[7])
                    cursor.execute(query)
                    health = cursor.fetchone()[0]
                    query = "UPDATE Parameters SET current_boss_health = {} where event_id = {}".format(record[2] + health, record[7])
                    cursor.execute(query)
                    con.commit()  
                    await session.send('回档成功，当前Boss血量回档至：{}'.format(record[2] + health))
                    return
                else:
                    await session.send('回档尝试被取消')
                    return
            elif record[3] == 'Last_Hit':
                prompt = '请确认是否回档以下记录：\n    {} 完成对第{}轮的Boss: {}的击杀，造成伤害{}\n    时间：{}'.format(record[5], record[1], record[6], record[2], record[4])
                confirmation = session.get('confirmation', prompt=prompt)
                if confirmation == '确认':
                    query = "UPDATE Battles SET status = 'Abandoned' where battle_id = {}".format(record[0])
                    cursor.execute(query)
                    query = 'SELECT current_boss_id, first_boss_id, last_boss_id, current_cycle from Parameters where event_id = {}'.format(record[7])
                    cursor.execute(query)
                    boss_record = cursor.fetchone()
                    cycle_number = boss_record[3]
                    current_boss_id = boss_record[0]
                    if current_boss_id == boss_record[1]:
                        current_boss_id = boss_record[2]
                        cycle_number -= 1
                    else:
                        current_boss_id -= 1
                    query = "UPDATE Parameters SET current_boss_health = {}, current_boss_id = {}, current_cycle = {} where event_id = {}".format(record[2], current_boss_id, cycle_number, record[7])
                    cursor.execute(query)
                    con.commit()  
                    await session.send('回档成功，当前Boss回档为第{}轮的{}，血量回档至：{}'.format(cycle_number, record[6], record[2]))
                    return
                else:
                    await session.send('回档尝试被取消')
                    return


@on_command('change_health', aliases=('血量重设', '重设血量'), only_to_me=False)
async def change_health(session: CommandSession):
    if session.ctx['message_type'] == 'group':
        user_id=session.ctx['user_id'] 
        if not_supervisor(user_id):
            await session.send('血量重设仅限指挥使用')
            return
        else:
            if session.current_arg_text == '' or not session.current_arg_text.isnumeric():
                await session.send('请附带Boss当前血量')
                return
            else:
                con = database_connection()
                cursor = con.cursor()
                query = "UPDATE Parameters SET current_boss_health = {} where status = 'Active'".format(int(session.current_arg_text))
                cursor.execute(query)
                con.commit()  
                await session.send('Boss血量重设成功')
                return

@on_command('set_sl', aliases=('sl', 'SL'), only_to_me=False)
async def set_sl(session: CommandSession):
    if session.ctx['message_type'] == 'group':
        x=re.search(r".*\s*\[CQ\:at,qq\=(\d+)\]", session.ctx['raw_message'])      
        user_id=session.ctx['user_id']
        if x != None:
            user_id = x.group(1)
        con = database_connection()
        cursor = con.cursor()
        query = "INSERT INTO SL_Record (member_id) VALUES ({})".format(user_id)
        cursor.execute(query)
        con.commit()
        await session.send('{} SL已记录'.format(get_nickname(user_id)))

@on_command('check_sl', aliases=('查sl', '查SL'), only_to_me=False)
async def set_sl(session: CommandSession):
    if session.ctx['message_type'] == 'group':
        user_id=session.ctx['user_id']
        con = database_connection()
        cursor = con.cursor()
        query = "SELECT DISTINCT Member.nickname from SL_Record inner join Member on SL_Record.member_id = Member.member_id where SL_Record.record_time > '{}'".format(get_refresh_time())
        cursor.execute(query)
        list = cursor.fetchall()
        response = '今日已使用过SL的有： '
        for member in list:
            response += '{}； '.format(member[0])
        await session.send(response)

@on_command('h_pic', aliases=('有涩图功能吗', '有色图功能吗', '涩图', '色图'), only_to_me=False)
async def h_pic(session: CommandSession):
    await session.set_group_ban(session.ctx['group_id'], session.ctx['user_id'], 60)

@on_command('show_damage', aliases=('查刀', '查询', '出刀列表'), only_to_me=False)
async def show_damage(session: CommandSession):
    await session.send('site address')

@on_command('show_guide', aliases=('作业', '查轴', '攻略'), only_to_me=False)
async def show_guide(session: CommandSession):
    await session.send('site address')

def database_connection():
    mydb = mysql.connector.connect(
        ##
    )
    return mydb

def get_nickname(user_id):
    con = database_connection()
    cursor = con.cursor()
    query = "SELECT nickname FROM Member where member_id = {}".format(user_id)
    cursor.execute(query)
    nickname = cursor.fetchone()
    if nickname is None:
        return ''
    else:
        return nickname[0]

def find_battle_record(user_id):
    con = database_connection()
    cursor = con.cursor()
    query = "SELECT battle_id FROM Battles where (status = 'Running' or status = 'OnTree') and (member_id = {})".format(user_id)
    cursor.execute(query)
    battle_id = cursor.fetchone()
    return battle_id

def update_parameters():
    con = database_connection()
    cursor = con.cursor()
    query = "SELECT Parameters.event_id, Parameters.current_boss_id, Parameters.current_cycle, Parameters.current_boss_health, Parameters.first_boss_id, Parameters.last_boss_id, Boss.boss_name from Boss inner join Parameters on Boss.boss_id = Parameters.current_boss_id where Parameters.status = 'Active'"
    cursor.execute(query)
    result = cursor.fetchone()
    if result[1] == result[5]:
        new_boss_id = result[4]
        cycle_number = result[2] + 1
    else:
        new_boss_id = result[1] + 1
        cycle_number = result[2]
    print('new boss id: {}, new cycle number: {}'.format(new_boss_id, cycle_number))
    query = "SELECT health_pool FROM Boss where boss_id = {}".format(new_boss_id)
    cursor.execute(query)
    new_health_pool = cursor.fetchone()[0]
    query = "UPDATE Parameters SET current_boss_id = {}, current_cycle = {}, current_boss_health = {} where event_id = {}".format(new_boss_id, cycle_number, new_health_pool, result[0])
    cursor.execute(query)
    con.commit()
    return result

def final_message(user_id, boss_name):
    nickname = get_nickname(user_id)   
    con = database_connection()
    cursor = con.cursor()
    query = "SELECT Boss.boss_name, Parameters.current_cycle, Parameters.current_boss_health from Boss inner join Parameters on Boss.boss_id = Parameters.current_boss_id where Parameters.status = 'Active'"
    cursor.execute(query)
    new_result = cursor.fetchone()
    return '{} 已成功击杀Boss: {}\n当前boss为{}，第{}周目，血量： {}/{}'.format(nickname, boss_name, new_result[0], new_result[1], new_result[2], new_result[2])

def cut_tree():
    con = database_connection()
    cursor = con.cursor()         
    query = "UPDATE Battles SET status = 'Abandoned' where status = 'Running' or status = 'OnTree'"
    cursor.execute(query)
    con.commit()

def check_total_runs(user_id):
    today_start_str = get_refresh_time()
    con = database_connection()
    cursor = con.cursor()
    query = "SELECT count(battle_id) FROM Battles where (status = 'Finished' or status = 'OffTree') and (record_time > '{}' and member_id = {})".format(today_start_str, user_id)
    print(query)
    cursor.execute(query)
    return cursor.fetchone()[0]

def get_refresh_time():
    tz = pytz.timezone('Asia/Shanghai')
    today_start_str = datetime.now(tz).strftime("%Y-%m-%d")+' 05:00:00.000000+08:00'
    today_start = datetime.fromisoformat(today_start_str)
    now = datetime.now(tz)
    #now = datetime.fromisoformat('2020-05-02 04:00:00.000000+08:00')
    if now < today_start:
        today_start -= timedelta(days = 1)
    return today_start.strftime("%Y-%m-%d")+'T05:00:00'

def not_supervisor(user_id):
    con = database_connection()
    cursor = con.cursor()         
    query = "SELECT role FROM Member where member_id = {}".format(user_id)
    cursor.execute(query)
    role = cursor.fetchone()
    if role[0] == 'member':
        return True
    else:
        return False

async def start_new_run(user_id):
    if check_total_runs(user_id) >= 3:
        return '您今日已出满三刀，请不要调戏Saren，不然打飞你哦'
    nickname = get_nickname(user_id)
    con = database_connection()
    cursor = con.cursor()
    query = "SELECT count(*) FROM Battles where status = 'Running'"
    cursor.execute(query)
    runningNum = cursor.fetchone()
    query = "SELECT Parameters.max_running_member_allowed, Parameters.event_id, Parameters.current_boss_id, Boss.boss_name, Parameters.current_cycle, Parameters.current_boss_health, Boss.health_pool from Boss inner join Parameters on Boss.boss_id = Parameters.current_boss_id where Parameters.status = 'Active'"
    cursor.execute(query)
    result = cursor.fetchone()
    if runningNum[0] < result[0]:
        query = "SELECT count(*) FROM Battles where (status = 'Running' or status = 'OnTree') and (member_id = {})".format(user_id)
        cursor.execute(query)
        if cursor.fetchone()[0] > 0:
            return '您已出刀，请勿重复出刀'
        else:
            query = "INSERT INTO Battles (member_id, event_id, boss_id, cycle_number, status) VALUES (%s, %s, %s, %s, %s)"
            val = (user_id, result[1], result[2], result[4], 'Running')
            cursor.execute(query, val)
            con.commit()
            return '{} 开始出刀\n当前boss为{}，第{}周目，血量： {}/{}'.format(nickname, result[3],result[4],result[5],result[6])
    else:
        query = "SELECT Member.nickname FROM Battles inner join Member on Battles.member_id = Member.member_id where Battles.status = 'Running'"
        cursor.execute(query)
        running_members = cursor.fetchall()
        response = '当前 '
        for member in running_members:
            response += member[0] + ' '
        response += '正在出刀，请耐心等候'
        return response

async def end_run(user_id, player_id, damage, finish_type, session):
    damage = damage.strip()
    if damage is None or not damage.isnumeric():
        return '您提交的{}指令并没有附带合法伤害值\n请使用以下指令：\n    {} XXXXX\n    XXXXX-为本次出刀造成的伤害\n如果您本次出的是尾刀，请使用指令： 尾刀'.format(finish_type, finish_type)
    else:
        damage = int(damage)
        if damage < 100000:
            await session.send('伤害怎么这么低，你个臭弟弟给我挂树去！')
        nickname = get_nickname(user_id)
        con = database_connection()
        cursor = con.cursor()
        query = "SELECT Parameters.current_boss_health, Boss.boss_name, Parameters.current_cycle, Boss.health_pool, Parameters.event_id, Parameters.current_boss_id from Boss inner join Parameters on Boss.boss_id = Parameters.current_boss_id where Parameters.status = 'Active'"
        cursor.execute(query)
        result = cursor.fetchone()
        if damage >= result[0]:
            return '您本次造成的伤害高于当前BOSS剩余血量，尾刀请使用指令： 尾刀'
        else:
            battle_id = find_battle_record(user_id)
            if battle_id is None:
                # if check_total_runs(user_id) >= 3:
                #     return '您今日已出满三刀，请不要调戏Saren，不然打飞你哦'                       
                # confirmation = session.get('confirmation', prompt='您还没有出刀，请确认您是否自闭了')
                # if confirmation == '确认':
                #     query = "INSERT INTO Battles (member_id, event_id, boss_id, cycle_number, damage, status) VALUES (%s, %s, %s, %s, %s)"
                #     val = (user_id, result[4], result[5], result[2], damage, 'Finished')
                #     cursor.execute(query, val)
                #     query = "UPDATE Parameters SET current_boss_health = {}".format(result[0]-damage)
                #     cursor.execute(query)
                #     con.commit()
                #     return '{} {}\n当前boss为{}，第{}周目，血量： {}/{}'.format(nickname, finish_type, result[1],result[2],result[0]-damage,result[3])
                # else:
                #     print('本次完成出刀没有记录')
                #     return '本次完成出刀没有记录'
                return '您还没有出刀，请先出刀'
            else:
                if (player_id is None):
                    query = "UPDATE Battles SET damage = {}, status = 'Finished' where battle_id = {}".format(damage, battle_id[0])
                else:
                    query = "UPDATE Battles SET damage = {}, status = 'Finished', player_id = {} where battle_id = {}".format(damage, player_id, battle_id[0])
                print(query)
                cursor.execute(query)
                query = "UPDATE Parameters SET current_boss_health = {}".format(result[0]-damage)
                cursor.execute(query)
                con.commit()                
                return '{} {}\n当前boss为{}，第{}周目，血量： {}/{}'.format(nickname, finish_type, result[1],result[2],result[0]-damage,result[3])