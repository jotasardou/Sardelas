"""Gera música e efeitos originais de NeuroRun. Execute da raiz do projeto.

Usa apenas a biblioteca padrão do Python; nenhuma gravação de terceiros.
"""
import math
import random
import struct
import wave
from pathlib import Path

RATE = 22050
OUT = Path(__file__).resolve().parents[1] / "Assets/NeuroRun/Audio"
OUT.mkdir(parents=True, exist_ok=True)
random.seed(12)


def save(name, values):
    with wave.open(str(OUT / (name + ".wav")), "wb") as file:
        file.setnchannels(1)
        file.setsampwidth(2)
        file.setframerate(RATE)
        file.writeframes(b"".join(struct.pack("<h", int(max(-1, min(1, v)) * 30000)) for v in values))


def note(midi):
    return 440 * 2 ** ((midi - 69) / 12)


def effect(name, notes, duration=.12):
    values = []
    for midi in notes:
        for i in range(int(RATE * duration)):
            t = i / RATE
            envelope = min(1, t / .008) * (1 - t / duration) ** 2
            tone = math.sin(2 * math.pi * note(midi) * t)
            values.append(tone * envelope * .6)
    save(name, values)


effect("Collect", [79, 86], .075)
effect("Correct", [72, 76, 79, 84], .10)
effect("Wrong", [55, 51], .16)
effect("Damage", [43, 39, 35], .08)
effect("Dash", [67, 74, 81], .04)
effect("Win", [60, 64, 67, 72, 76, 79, 84], .12)
effect("Click", [74], .07)

# Loop de oito compassos, com arpejo, baixo e pulso discreto (96 BPM).
beat = 60 / 96
length = 32 * beat
samples = [0.0] * int(length * RATE)
chords = [(48, 60, 63, 67), (44, 56, 60, 63), (51, 63, 67, 70), (46, 58, 62, 65)]
for step in range(64):
    start = step * beat / 2
    chord = chords[(step // 16) % 4]
    midi = chord[1 + step % 3] + 12
    for i in range(int(beat * .47 * RATE)):
        t = i / RATE
        env = min(1, t / .012) * math.exp(-t * 11)
        index = int(start * RATE) + i
        if index < len(samples):
            samples[index] += .13 * env * (math.sin(2 * math.pi * note(midi) * t) + .2 * math.sin(4 * math.pi * note(midi) * t))
    if step % 2 == 0:
        for i in range(int(.30 * RATE)):
            t = i / RATE
            index = int(start * RATE) + i
            if index < len(samples):
                samples[index] += .16 * math.exp(-t * 12) * math.sin(2 * math.pi * note(chord[0]) * t)
    if step % 4 == 0:
        for i in range(int(.12 * RATE)):
            t = i / RATE
            index = int(start * RATE) + i
            if index < len(samples):
                samples[index] += .11 * math.exp(-t * 32) * math.sin(2 * math.pi * (65 * t + 25 * .04 * (1 - math.exp(-t / .04))))
save("LabLoop", samples)
print("Música e 7 efeitos gerados em", OUT)
