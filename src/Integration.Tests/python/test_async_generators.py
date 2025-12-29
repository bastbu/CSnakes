from collections.abc import AsyncGenerator


async def send_yield(length: int) -> AsyncGenerator[str, int]:
    for i in range(length):
        x = yield f"Item {i}"
        if x:
            yield f"Received {x}"


async def yield_only() -> AsyncGenerator[str, None]:
    yield "one"
    yield "two"


async def erroneous_cleanup() -> AsyncGenerator[str, None]:
    try:
        yield "one"
    finally:
        raise Exception("Cleanup error")
