import os
import sys
from pathlib import Path
import threading

os.environ["PYGAME_HIDE_SUPPORT_PROMPT"] = "1"
sys.path.insert(0, str(Path(__file__).resolve().parent / "MoreTelloLink"))

from telloLink.Dron import Dron

def main():
    tello = Dron().tello
    try:
        tello.connect()
        battery = tello.get_current_state().get("bat")
        print("" if battery is None else battery)
    finally:
        tello.end()

def stop_when_unity_closes_input():
    sys.stdin.read()
    os._exit(0)

if __name__ == "__main__":
    if "--unity" in sys.argv:
        threading.Thread(target=stop_when_unity_closes_input, daemon=True).start()

    main()