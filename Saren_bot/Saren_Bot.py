from os import path
import nonebot
import config
from nonebot import on_command, CommandSession

if __name__ == '__main__':
    nonebot.init(config)
    nonebot.load_plugins(
        path.join(path.dirname(__file__), 'Saren', 'plugins'),
        'Saren.plugins'
    )
    nonebot.run(host='127.0.0.1', port=8080)
