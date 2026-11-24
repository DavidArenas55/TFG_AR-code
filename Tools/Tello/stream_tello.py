import argparse
import os
import sys
import threading
from http.server import BaseHTTPRequestHandler, HTTPServer
from pathlib import Path

import cv2

os.environ["PYGAME_HIDE_SUPPORT_PROMPT"] = "1"
sys.path.insert(0, str(Path(__file__).resolve().parent / "MoreTelloLink"))

from telloLink.Dron import Dron


class FrameHandler(BaseHTTPRequestHandler):
    def setup(self):
        self.request.settimeout(1)
        super().setup()

    def do_GET(self):
        if self.path != "/frame.jpg":
            self.send_error(404)
            return

        frame = self.server.drone.frame_reader.frame
        if frame is None:
            self.send_error(503, "Esperando imagen del Tello")
            return

        bgr = cv2.cvtColor(frame, cv2.COLOR_RGB2BGR)
        encoded, jpeg = cv2.imencode(".jpg", bgr)
        if not encoded:
            self.send_error(503, "Imagen no disponible")
            return

        self.send_response(200)
        self.send_header("Content-Type", "image/jpeg")
        self.send_header("Content-Length", str(jpeg.nbytes))
        self.send_header("Cache-Control", "no-store")
        self.end_headers()
        self.wfile.write(jpeg.tobytes())

    def log_message(self, format, *args):
        # Unity consulta este endpoint repetidamente.
        pass


def watch_unity_input(stop_requested):
    sys.stdin.read()
    stop_requested.set()


def main():
    parser = argparse.ArgumentParser(description="Sirve imágenes del Tello en localhost.")
    parser.add_argument("--unity", action="store_true", help="Detiene el vídeo al cerrar stdin.")
    args = parser.parse_args()

    stop_requested = threading.Event()
    if args.unity:
        threading.Thread(target=watch_unity_input, args=(stop_requested,), daemon=True).start()

    address = ("127.0.0.1", 8765)
    with HTTPServer(address, FrameHandler) as server:
        server.timeout = 0.25
        drone = Dron()
        server.drone = drone
        try:
            drone.tello.connect()
            if stop_requested.is_set():
                return

            drone.start_video()
            print(f"Vídeo disponible en http://{address[0]}:{address[1]}/frame.jpg", flush=True)
            while not stop_requested.is_set():
                server.handle_request()
        finally:
            # end() apaga la emisión si sigue activa y detiene el lector de vídeo.
            drone.tello.end()


if __name__ == "__main__":
    try:
        main()
    except KeyboardInterrupt:
        pass
