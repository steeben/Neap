"""Draws Turt's tray glyphs from the mark and writes every raster.

Run from the repository root: python tools/brand/render.py (needs cairosvg and Pillow).
The mark, assets/brand/turt-mark.svg, is the master; the tray glyphs are the
same turtle without its tile, written beside it as SVG.
"""
import io
import re
from pathlib import Path

import cairosvg
from PIL import Image

OUTLINE = "#EFEFF1"
BODY = "#1A1A22"

# The turtle without its tile, centred and filling the square: its sticker
# outline spans about 6 to 504 across and 80 to 500 down in the mark's box.
TRAY_TRANSFORM = 'translate(1, -34) scale(1)'


def tray_svg(mark, outline):
    """The mark without its tile, the turtle filling the box, its sticker outline in the given colour."""
    svg = re.sub(r"<rect [^>]*/>", "", mark, count=1)
    svg = svg.replace('transform="translate(36, 28) scale(0.86)"', f'transform="{TRAY_TRANSFORM}"', 1)
    return svg.replace(f'id="white-outline" stroke="{OUTLINE}"', f'id="white-outline" stroke="{outline}"', 1) \
              .replace(f'stroke-linejoin="round" fill="{OUTLINE}">', f'stroke-linejoin="round" fill="{outline}">', 1)


def raster(svg, size):
    png = cairosvg.svg2png(bytestring=svg.encode(), output_width=size, output_height=size)
    return Image.open(io.BytesIO(png)).convert("RGBA")


def ico(path, svg, sizes):
    images = [raster(svg, s) for s in sizes]
    biggest = images[-1]
    biggest.save(path, format="ICO", sizes=[(i.width, i.width) for i in images], append_images=images[:-1])


def main():
    root = Path.cwd()
    brand = root / "assets" / "brand"
    mark = (brand / "turt-mark.svg").read_text()
    # On a dark taskbar the white sticker outline sets the turtle off; on a
    # light one it would vanish, so the outline takes the body's shade.
    trays = {"dark": tray_svg(mark, OUTLINE), "light": tray_svg(mark, BODY)}
    for name, svg in trays.items():
        (brand / f"turt-tray-{name}.svg").write_text(svg)

    assets = root / "src" / "Neap.Desktop" / "Assets"
    ico(assets / "app.ico", mark, (16, 20, 24, 32, 48, 64, 128, 256))
    raster(mark, 128).save(assets / "logo.png")
    raster(mark, 512).save(root / "docs" / "neap.png")
    raster(mark, 256).save(root / "packaging" / "linux" / "neap.png")
    for name, svg in trays.items():
        ico(assets / f"tray-{name}.ico", svg, (16, 20, 24, 32, 40, 48))


if __name__ == "__main__":
    main()
