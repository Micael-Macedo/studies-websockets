import asyncio
from websockets.asyncio.server import serve
from datetime import datetime


async def echo(websocket):
    async for message in websocket:
        print(f"{datetime.now().strftime("%d/%m/%Y %H:%M:%S")} | Message Received: {message}");
        await websocket.send(message);


async def main():
    print("Server is Up!");
    async with serve(echo, "localhost", 8765) as server:
        await server.serve_forever();


if __name__ == "__main__":
    asyncio.run(main());