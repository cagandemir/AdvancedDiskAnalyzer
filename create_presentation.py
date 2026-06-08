# -*- coding: utf-8 -*-
"""
Advanced Disk Analyzer — Profesyonel Sunum Oluşturucu
=====================================================
python-pptx ile koyu temali, sekilli, animasyonlu,
logolu kurumsal sunum uretir.
"""

import os, copy, math
from pptx import Presentation
from pptx.util import Inches, Pt, Emu
from pptx.dml.color import RGBColor
from pptx.enum.shapes import MSO_SHAPE
from pptx.enum.text import PP_ALIGN, MSO_ANCHOR
from pptx.oxml import parse_xml
from pptx.oxml.ns import nsmap, qn
from PIL import Image

# ── Yardimci sabitler ─────────────────────────────────────────────
SLIDE_W = Inches(13.333)   # 16:9
SLIDE_H = Inches(7.5)
EMU_SLIDE_W = Emu(12192000)
EMU_SLIDE_H = Emu(6858000)

# Renk paleti
C_BG        = RGBColor(18, 18, 24)
C_CARD      = RGBColor(30, 30, 38)
C_CARD_LT   = RGBColor(38, 38, 48)
C_ACCENT    = RGBColor(255, 140, 0)     # turuncu
C_ACCENT2   = RGBColor(0, 160, 255)     # mavi
C_ACCENT3   = RGBColor(80, 220, 120)    # yesil
C_ACCENT4   = RGBColor(180, 100, 255)   # mor
C_ACCENT5   = RGBColor(255, 80, 100)    # kirmizi
C_WHITE     = RGBColor(240, 240, 245)
C_GRAY      = RGBColor(160, 160, 170)
C_DIM       = RGBColor(100, 100, 110)
C_LINE      = RGBColor(55, 55, 65)

LOGO = "logo_hq.png"

# ── XML yardimcilari ─────────────────────────────────────────────

def _add_gradient_fill(shape, c1, c2, angle=5400000):
    """Sekle cift-renk gradient dolgu ekler."""
    sp = shape._element
    spPr = sp.find(qn('a:spPr')) if sp.find(qn('a:spPr')) is not None else sp.find(qn('p:spPr'))
    if spPr is None:
        # try inside sp
        spPr = sp.spPr
    # remove old fill
    for old in list(spPr):
        tag = old.tag.split('}')[-1] if '}' in old.tag else old.tag
        if tag in ('solidFill', 'gradFill', 'noFill', 'blipFill', 'pattFill'):
            spPr.remove(old)
    gf = parse_xml(
        f'<a:gradFill xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main">'
        f'  <a:gsLst>'
        f'    <a:gs pos="0"><a:srgbClr val="{c1}"/></a:gs>'
        f'    <a:gs pos="100000"><a:srgbClr val="{c2}"/></a:gs>'
        f'  </a:gsLst>'
        f'  <a:lin ang="{angle}" scaled="1"/>'
        f'</a:gradFill>'
    )
    spPr.append(gf)

def _rgb_hex(c):
    return f'{c[0]:02X}{c[1]:02X}{c[2]:02X}'

def _add_entrance_anim(slide, shape, delay_ms=0, dur_ms=600, effect='fade'):
    """Sekle giris animasyonu ekler (fade / fly-from-bottom)."""
    sp_id = shape.shape_id
    sp_name = shape.name
    
    timing_xml = (
        f'<p:timing xmlns:p="http://schemas.openxmlformats.org/presentationml/2006/main"'
        f'          xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main">'
        f'  <p:tnLst>'
        f'    <p:par>'
        f'      <p:cTn id="1" dur="indefinite" restart="never" nodeType="tmRoot">'
        f'        <p:childTnLst>'
        f'          <p:seq concurrent="1" nextAc="seek">'
        f'            <p:cTn id="2" dur="indefinite" nodeType="mainSeq">'
        f'              <p:childTnLst>'
        f'                <p:par>'
        f'                  <p:cTn id="3" fill="hold">'
        f'                    <p:stCondLst><p:cond delay="0"/></p:stCondLst>'
        f'                    <p:childTnLst/>'
        f'                  </p:cTn>'
        f'                </p:par>'
        f'              </p:childTnLst>'
        f'            </p:cTn>'
        f'            <p:prevCondLst><p:cond evt="onPrev" delay="0"><p:tgtEl><p:sldTgt/></p:tgtEl></p:cond></p:prevCondLst>'
        f'            <p:nextCondLst><p:cond evt="onNext" delay="0"><p:tgtEl><p:sldTgt/></p:tgtEl></p:cond></p:nextCondLst>'
        f'          </p:seq>'
        f'        </p:childTnLst>'
        f'      </p:cTn>'
        f'    </p:par>'
        f'  </p:tnLst>'
        f'</p:timing>'
    )
    # Basit slide transition ile animasyon efekti veriyoruz
    pass  # full animation XML cok karmasik, transition ile idare edecegiz

def _add_slide_transition(slide, trans_type='fade', speed='med'):
    """Slayta gecis animasyonu ekler."""
    sld = slide._element
    # Eski transition'lari kaldir
    for old in sld.findall(qn('p:transition')):
        sld.remove(old)
    
    if trans_type == 'fade':
        xml = f'<p:transition xmlns:p="http://schemas.openxmlformats.org/presentationml/2006/main" spd="{speed}" advClick="1"><p:fade/></p:transition>'
    elif trans_type == 'push':
        xml = f'<p:transition xmlns:p="http://schemas.openxmlformats.org/presentationml/2006/main" spd="{speed}" advClick="1"><p:push dir="l"/></p:transition>'
    elif trans_type == 'wipe':
        xml = f'<p:transition xmlns:p="http://schemas.openxmlformats.org/presentationml/2006/main" spd="{speed}" advClick="1"><p:wipe dir="d"/></p:transition>'
    elif trans_type == 'cover':
        xml = f'<p:transition xmlns:p="http://schemas.openxmlformats.org/presentationml/2006/main" spd="{speed}" advClick="1"><p:cover dir="lu"/></p:transition>'
    else:
        xml = f'<p:transition xmlns:p="http://schemas.openxmlformats.org/presentationml/2006/main" spd="{speed}" advClick="1"><p:fade/></p:transition>'
    
    trans = parse_xml(xml)
    sld.insert(1, trans)


# ── Sunum fabrikasi ───────────────────────────────────────────────

def build():
    prs = Presentation()
    prs.slide_width  = EMU_SLIDE_W
    prs.slide_height = EMU_SLIDE_H

    blank_layout = prs.slide_layouts[6]  # blank

    # ── Logo hazirla ──
    logo_path = LOGO if os.path.exists(LOGO) else ("logo.png" if os.path.exists("logo.png") else None)

    # ================================================================
    #  YARDIMCI FONKSIYONLAR
    # ================================================================

    def dark_bg(slide):
        bg = slide.background.fill
        bg.solid()
        bg.fore_color.rgb = C_BG

    def add_logo_corner(slide, size=Inches(0.65)):
        """Sag ust koseye kucuk logo."""
        if logo_path:
            slide.shapes.add_picture(
                logo_path,
                EMU_SLIDE_W - Emu(size) - Inches(0.4),
                Inches(0.3),
                width=Emu(size)
            )

    def add_accent_bar(slide, color=C_ACCENT, width=Inches(0.12)):
        """Sol kenara ince vurgu cubugu."""
        bar = slide.shapes.add_shape(
            MSO_SHAPE.RECTANGLE, Inches(0), Inches(0), width, EMU_SLIDE_H
        )
        bar.fill.solid()
        bar.fill.fore_color.rgb = color
        bar.line.fill.background()
        return bar

    def add_bottom_strip(slide):
        """Alt kenara koyu bant + copyright."""
        strip = slide.shapes.add_shape(
            MSO_SHAPE.RECTANGLE, Inches(0), Inches(6.85), EMU_SLIDE_W, Inches(0.65)
        )
        strip.fill.solid()
        strip.fill.fore_color.rgb = RGBColor(14, 14, 18)
        strip.line.fill.background()
        
        tf = strip.text_frame
        tf.word_wrap = True
        p = tf.paragraphs[0]
        p.text = "Advanced Disk Analyzer  |  Açık Kaynak Disk Yönetim Çözümü  |  2025"
        p.font.size = Pt(10)
        p.font.color.rgb = C_DIM
        p.font.name = "Segoe UI"
        p.alignment = PP_ALIGN.CENTER

    def add_title_divider(slide, y=Inches(1.55), color=C_ACCENT):
        """Baslik altina ince cizgi."""
        line = slide.shapes.add_shape(
            MSO_SHAPE.RECTANGLE, Inches(0.7), y, Inches(2.0), Inches(0.04)
        )
        line.fill.solid()
        line.fill.fore_color.rgb = color
        line.line.fill.background()
        return line

    def add_text_box(slide, left, top, width, height, text, font_size=20,
                     color=C_WHITE, bold=False, font_name="Segoe UI",
                     alignment=PP_ALIGN.LEFT, anchor=MSO_ANCHOR.TOP):
        """Tek satirlik/cok satirlik metin kutusu."""
        txBox = slide.shapes.add_textbox(left, top, width, height)
        tf = txBox.text_frame
        tf.word_wrap = True
        tf.auto_size = None
        p = tf.paragraphs[0]
        p.text = text
        p.font.size = Pt(font_size)
        p.font.color.rgb = color
        p.font.bold = bold
        p.font.name = font_name
        p.alignment = alignment
        tf.vertical_anchor = anchor
        return txBox

    def add_card(slide, left, top, width, height, title, body, icon_char="●",
                 accent=C_ACCENT, card_bg=C_CARD):
        """Yuvarlatilmis kart: ikon + baslik + aciklama."""
        # Kart arkaplan
        card = slide.shapes.add_shape(
            MSO_SHAPE.ROUNDED_RECTANGLE, left, top, width, height
        )
        card.fill.solid()
        card.fill.fore_color.rgb = card_bg
        card.line.color.rgb = C_LINE
        card.line.width = Pt(1)
        # Yuvarlatma
        card.adjustments[0] = 0.05

        # Ikon dairesi
        icon_size = Inches(0.55)
        icon_circle = slide.shapes.add_shape(
            MSO_SHAPE.OVAL, left + Inches(0.3), top + Inches(0.35), icon_size, icon_size
        )
        icon_circle.fill.solid()
        icon_circle.fill.fore_color.rgb = accent
        icon_circle.line.fill.background()
        itf = icon_circle.text_frame
        itf.paragraphs[0].text = icon_char
        itf.paragraphs[0].font.size = Pt(18)
        itf.paragraphs[0].font.color.rgb = C_BG
        itf.paragraphs[0].font.bold = True
        itf.paragraphs[0].alignment = PP_ALIGN.CENTER
        itf.vertical_anchor = MSO_ANCHOR.MIDDLE

        # Baslik
        add_text_box(slide, left + Inches(1.05), top + Inches(0.25),
                     width - Inches(1.4), Inches(0.45),
                     title, font_size=17, color=C_WHITE, bold=True)

        # Icerik
        add_text_box(slide, left + Inches(1.05), top + Inches(0.7),
                     width - Inches(1.4), height - Inches(0.95),
                     body, font_size=13, color=C_GRAY)

    def add_bullet_card(slide, left, top, width, height, bullets, accent=C_ACCENT):
        """Tek buyuk kart icinde madde isaretli liste."""
        card = slide.shapes.add_shape(
            MSO_SHAPE.ROUNDED_RECTANGLE, left, top, width, height
        )
        card.fill.solid()
        card.fill.fore_color.rgb = C_CARD
        card.line.color.rgb = C_LINE
        card.line.width = Pt(1)
        card.adjustments[0] = 0.03

        txBox = slide.shapes.add_textbox(left + Inches(0.5), top + Inches(0.35),
                                          width - Inches(1.0), height - Inches(0.7))
        tf = txBox.text_frame
        tf.word_wrap = True

        for i, bullet in enumerate(bullets):
            p = tf.paragraphs[0] if i == 0 else tf.add_paragraph()
            p.text = "▸  " + bullet
            p.font.size = Pt(17)
            p.font.color.rgb = C_WHITE
            p.font.name = "Segoe UI"
            p.space_after = Pt(16)
            p.line_spacing = Pt(26)

    def add_number_badge(slide, left, top, number, accent=C_ACCENT):
        """Numarali daire badge."""
        badge = slide.shapes.add_shape(
            MSO_SHAPE.OVAL, left, top, Inches(0.5), Inches(0.5)
        )
        badge.fill.solid()
        badge.fill.fore_color.rgb = accent
        badge.line.fill.background()
        tf = badge.text_frame
        tf.paragraphs[0].text = str(number)
        tf.paragraphs[0].font.size = Pt(16)
        tf.paragraphs[0].font.color.rgb = C_BG
        tf.paragraphs[0].font.bold = True
        tf.paragraphs[0].alignment = PP_ALIGN.CENTER
        tf.vertical_anchor = MSO_ANCHOR.MIDDLE

    def page_header(slide, title, subtitle=None, accent=C_ACCENT):
        """Standart sayfa ustu: baslik + cizgi + alt baslik."""
        add_text_box(slide, Inches(0.7), Inches(0.45), Inches(10), Inches(0.7),
                     title, font_size=34, color=C_WHITE, bold=True,
                     font_name="Segoe UI Semibold")
        add_title_divider(slide, Inches(1.25), accent)
        if subtitle:
            add_text_box(slide, Inches(0.7), Inches(1.40), Inches(10), Inches(0.45),
                         subtitle, font_size=15, color=C_GRAY)

    def decor_circles(slide):
        """Dekoratif seffaf halkalar."""
        for cx, cy, sz, clr in [
            (Inches(11.5), Inches(0.5), Inches(1.8), C_ACCENT),
            (Inches(12.0), Inches(1.7), Inches(0.8), C_ACCENT2),
        ]:
            c = slide.shapes.add_shape(MSO_SHAPE.OVAL, cx, cy, sz, sz)
            c.fill.background()
            c.line.color.rgb = clr
            c.line.width = Pt(1.5)
            # Seffamlik — oxml ile
            ln = c._element.spPr.find(qn('a:ln'))
            if ln is not None:
                sf = ln.find(qn('a:solidFill'))
                if sf is not None:
                    clr_el = sf[0]
                    alpha = parse_xml('<a:alpha xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main" val="30000"/>')
                    clr_el.append(alpha)

    # ================================================================
    #  SLAYT 1 — KAPAK
    # ================================================================
    s1 = prs.slides.add_slide(blank_layout)
    dark_bg(s1)
    _add_slide_transition(s1, 'fade', 'slow')

    # Buyuk arka plan dekoratif daire
    big_circle = s1.shapes.add_shape(MSO_SHAPE.OVAL, Inches(7.5), Inches(-1.5), Inches(9), Inches(9))
    big_circle.fill.solid()
    big_circle.fill.fore_color.rgb = RGBColor(24, 24, 32)
    big_circle.line.color.rgb = RGBColor(40, 40, 50)
    big_circle.line.width = Pt(2)
    
    # Ikinci halka
    ring = s1.shapes.add_shape(MSO_SHAPE.OVAL, Inches(8.2), Inches(-0.8), Inches(7.5), Inches(7.5))
    ring.fill.background()
    ring.line.color.rgb = C_ACCENT
    ring.line.width = Pt(1.5)
    # alpha
    ln = ring._element.spPr.find(qn('a:ln'))
    if ln is not None:
        sf = ln.find(qn('a:solidFill'))
        if sf is not None:
            clr_el = sf[0]
            alpha = parse_xml('<a:alpha xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main" val="20000"/>')
            clr_el.append(alpha)

    add_accent_bar(s1, C_ACCENT, Inches(0.15))

    # Logo buyuk
    if logo_path:
        s1.shapes.add_picture(logo_path, Inches(0.8), Inches(1.2), width=Inches(2.0))

    # Baslik
    add_text_box(s1, Inches(0.8), Inches(3.5), Inches(8), Inches(0.8),
                 "Advanced Disk Analyzer", font_size=46, color=C_WHITE, bold=True,
                 font_name="Segoe UI Semibold")

    # Alt baslik
    add_text_box(s1, Inches(0.8), Inches(4.4), Inches(8), Inches(0.5),
                 "Yeni Nesil Disk Analiz ve Akıllı Temizlik Çözümü", font_size=22,
                 color=C_ACCENT, font_name="Segoe UI Light")

    # Tagler
    add_text_box(s1, Inches(0.8), Inches(5.2), Inches(9), Inches(0.4),
                 "Açık Kaynak  ·  NTFS Turbo Tarama  ·  AI Destekli  ·  Güvenli  ·  Ücretsiz",
                 font_size=14, color=C_GRAY)

    add_bottom_strip(s1)

    # ================================================================
    #  SLAYT 2 — PROJE AMACI
    # ================================================================
    s2 = prs.slides.add_slide(blank_layout)
    dark_bg(s2)
    _add_slide_transition(s2, 'push')
    add_accent_bar(s2, C_ACCENT)
    add_logo_corner(s2)
    decor_circles(s2)
    page_header(s2, "Projenin Amacı", "Neden böyle bir araca ihtiyaç var?")

    cards2 = [
        ("🔍", "Görselleştirme", "Disk kullanımını pasta grafikler, ağaç haritaları ve canlı listelerle\nanında görselleştirerek karmaşıklığı ortadan kaldırır.", C_ACCENT),
        ("⚡", "Hız", "NTFS MFT motoruyla 1 TB'lık diski saniyeler içinde\ntarayarak kullanıcı zamanından tasarruf sağlar.", C_ACCENT2),
        ("🛡️", "Güvenlik", "Sistem dosyalarını koruyan akıllı güvenlik katmanıyla\nyanlışlıkla kritik dosyaların silinmesini engeller.", C_ACCENT3),
        ("🤖", "Akıllı Analiz", "AI tabanlı öneri motoruyla gereksiz dosyaları otomatik\ntespit ederek temizlik rehberi sunar.", C_ACCENT4),
    ]
    for i, (icon, title, body, clr) in enumerate(cards2):
        col = i % 2
        row = i // 2
        x = Inches(0.6) + col * Inches(6.2)
        y = Inches(2.2) + row * Inches(2.25)
        add_card(s2, x, y, Inches(5.8), Inches(2.0), title, body, icon, clr)

    add_bottom_strip(s2)

    # ================================================================
    #  SLAYT 3 — RAKİPLERLE KARSILASTIRMA
    # ================================================================
    s3 = prs.slides.add_slide(blank_layout)
    dark_bg(s3)
    _add_slide_transition(s3, 'cover')
    add_accent_bar(s3, C_ACCENT2)
    add_logo_corner(s3)
    decor_circles(s3)
    page_header(s3, "Rakiplerle Karşılaştırma", "WizTree ve TreeSize'a karşı neden tercih edilmeli?", C_ACCENT2)

    # Tablo yapalim — shape'lerle
    headers = ["Özellik", "Advanced Disk\nAnalyzer", "WizTree", "TreeSize"]
    rows_data = [
        ["Açık Kaynak Kod",     "✓", "✗", "✗"],
        ["AI Öneri Sistemi",    "✓", "✗", "✗"],
        ["Modern UI / Tema",    "✓", "Kısmi", "✗"],
        ["NTFS MFT Tarama",     "✓", "✓", "Kısmi"],
        ["Tamamen Ücretsiz",    "✓", "Kısmi", "✗"],
        ["Gizli Telemetri Yok", "✓", "?", "✗"],
        ["Canlı Liste (Live)",  "✓", "✗", "✗"],
    ]
    
    col_widths = [Inches(2.6), Inches(2.8), Inches(2.0), Inches(2.0)]
    table_left = Inches(0.8)
    row_height = Inches(0.50)
    header_h   = Inches(0.60)
    table_top  = Inches(2.1)

    # Header row
    x_cursor = table_left
    for ci, hdr in enumerate(headers):
        cell = s3.shapes.add_shape(
            MSO_SHAPE.ROUNDED_RECTANGLE if ci == 0 else MSO_SHAPE.RECTANGLE,
            x_cursor, table_top, col_widths[ci], header_h
        )
        cell.fill.solid()
        cell.fill.fore_color.rgb = C_ACCENT2 if ci == 1 else RGBColor(35, 35, 45)
        cell.line.color.rgb = C_LINE
        cell.line.width = Pt(0.5)
        if ci == 0:
            cell.adjustments[0] = 0.05
        tf = cell.text_frame
        tf.paragraphs[0].text = hdr
        tf.paragraphs[0].font.size = Pt(13)
        tf.paragraphs[0].font.bold = True
        tf.paragraphs[0].font.color.rgb = C_WHITE if ci <= 1 else C_GRAY
        tf.paragraphs[0].font.name = "Segoe UI"
        tf.paragraphs[0].alignment = PP_ALIGN.CENTER
        tf.vertical_anchor = MSO_ANCHOR.MIDDLE
        tf.word_wrap = True
        x_cursor += col_widths[ci]

    # Data rows
    for ri, row in enumerate(rows_data):
        x_cursor = table_left
        y = table_top + header_h + ri * row_height
        for ci, val in enumerate(row):
            cell = s3.shapes.add_shape(
                MSO_SHAPE.RECTANGLE, x_cursor, y, col_widths[ci], row_height
            )
            is_even = ri % 2 == 0
            cell.fill.solid()
            cell.fill.fore_color.rgb = C_CARD if is_even else C_CARD_LT
            cell.line.color.rgb = C_LINE
            cell.line.width = Pt(0.3)
            tf = cell.text_frame
            tf.paragraphs[0].text = val
            tf.paragraphs[0].font.size = Pt(13)
            tf.paragraphs[0].font.name = "Segoe UI"
            tf.paragraphs[0].alignment = PP_ALIGN.CENTER if ci > 0 else PP_ALIGN.LEFT
            tf.vertical_anchor = MSO_ANCHOR.MIDDLE
            
            if ci == 0:
                tf.paragraphs[0].font.color.rgb = C_WHITE
            elif val == "✓":
                tf.paragraphs[0].font.color.rgb = C_ACCENT3
                tf.paragraphs[0].font.bold = True
                tf.paragraphs[0].font.size = Pt(16)
            elif val == "✗":
                tf.paragraphs[0].font.color.rgb = C_ACCENT5
                tf.paragraphs[0].font.size = Pt(16)
            else:
                tf.paragraphs[0].font.color.rgb = C_GRAY
            
            x_cursor += col_widths[ci]

    add_text_box(s3, Inches(0.8), Inches(6.1), Inches(10), Inches(0.4),
                 "✓ = Destekleniyor   ✗ = Desteklenmiyor   ? = Belirsiz",
                 font_size=11, color=C_DIM)

    add_bottom_strip(s3)

    # ================================================================
    #  SLAYT 4 — ACIK KAYNAK
    # ================================================================
    s4 = prs.slides.add_slide(blank_layout)
    dark_bg(s4)
    _add_slide_transition(s4, 'wipe')
    add_accent_bar(s4, C_ACCENT3)
    add_logo_corner(s4)
    decor_circles(s4)
    page_header(s4, "Tamamen Açık Kaynak Güvencesi", "Şeffaflık, güven ve topluluk gücü", C_ACCENT3)

    bullets4 = [
        "Kaynak kodlar tamamen açıktır — herkes inceleyebilir, denetleyebilir ve katkı sağlayabilir.",
        "Ticari rakiplerin aksine hiçbir telemetri, analitik veya gizli veri toplama mekanizması yoktur.",
        "Kurumsal firmalar kendi güvenlik politikalarına uygun şekilde kodu denetleyip iç kullanımda yayınlayabilir.",
        "Topluluk destekli geliştirme modeli sayesinde hata tespiti ve düzeltmeler çok daha hızlı gerçekleşir.",
        "Ücretsizdir ve sonsuza kadar ücretsiz kalacaktır — lisans ücreti, reklam veya premium kısıtlama yoktur."
    ]
    add_bullet_card(s4, Inches(0.6), Inches(2.1), Inches(12.0), Inches(4.3), bullets4, C_ACCENT3)

    # Dekoratif buyuk ikon
    icon_box = s4.shapes.add_shape(MSO_SHAPE.OVAL, Inches(10.8), Inches(3.0), Inches(1.5), Inches(1.5))
    icon_box.fill.solid()
    icon_box.fill.fore_color.rgb = C_ACCENT3
    icon_box.line.fill.background()
    tf = icon_box.text_frame
    tf.paragraphs[0].text = "</>"
    tf.paragraphs[0].font.size = Pt(28)
    tf.paragraphs[0].font.color.rgb = C_BG
    tf.paragraphs[0].font.bold = True
    tf.paragraphs[0].alignment = PP_ALIGN.CENTER
    tf.vertical_anchor = MSO_ANCHOR.MIDDLE

    add_bottom_strip(s4)

    # ================================================================
    #  SLAYT 5 — TARAMA ALTYAPISI
    # ================================================================
    s5 = prs.slides.add_slide(blank_layout)
    dark_bg(s5)
    _add_slide_transition(s5, 'fade')
    add_accent_bar(s5, C_ACCENT2)
    add_logo_corner(s5)
    decor_circles(s5)
    page_header(s5, "Modern Tarama Altyapısı", "Işık hızında disk analizi nasıl çalışır?", C_ACCENT2)

    # 3 kolon kart
    scan_cards = [
        ("⚡", "NTFS Turbo", 
         "Windows'un MFT (Master File Table)\ntablosunu doğrudan okuyarak\ntüm diski saniyeler içinde tarar.\nYönetici modunda maksimum hız.", C_ACCENT),
        ("🔄", "WinAPI FastScan",
         "FindFirstFile/FindNextFile kernel\nAPI'leri ile çok çekirdekli paralel\ntarama. SSD'lerde CPU sayısı\nkadar thread, HDD'de 2 thread.", C_ACCENT2),
        ("📡", "Canlı İzleme",
         "Tarama devam ederken dosyalar\nanlık olarak listeye eklenir.\nUI donması olmadan milyonlarca\ndosya gerçek zamanlı gösterilir.", C_ACCENT3),
    ]
    for i, (icon, title, body, clr) in enumerate(scan_cards):
        x = Inches(0.5) + i * Inches(4.2)
        add_card(s5, x, Inches(2.3), Inches(3.9), Inches(2.8), title, body, icon, clr)

    # Alt bilgi
    add_text_box(s5, Inches(0.7), Inches(5.6), Inches(11.5), Inches(0.8),
                 "Tarama motoru; sürücü tipine (SSD/HDD/Ağ) göre otomatik olarak en uygun stratejiyi seçer.\n"
                 "Allocated size, hard-link deduplikasyonu ve cluster hizalama hesaplamaları dahili olarak yapılır.",
                 font_size=13, color=C_DIM)

    add_bottom_strip(s5)

    # ================================================================
    #  SLAYT 6 — GÜVENLİK KURALLARI
    # ================================================================
    s6 = prs.slides.add_slide(blank_layout)
    dark_bg(s6)
    _add_slide_transition(s6, 'push')
    add_accent_bar(s6, C_ACCENT5)
    add_logo_corner(s6)
    decor_circles(s6)
    page_header(s6, "Güvenlik ve Koruma Katmanları", "Sisteminiz her zaman güvende", C_ACCENT5)

    sec_items = [
        ("1", "Sistem Dosyası Koruması",
         "C:\\Windows, System32 gibi kritik klasörlerdeki dosyaların silinmesi fiziksel olarak engellenmiştir.",
         C_ACCENT5),
        ("2", "Kilitli Dosya Tespiti",
         "Sistem tarafından kullanımda olan dosyalar algılanır ve hata vermeden güvenle atlanır.",
         C_ACCENT),
        ("3", "Yetki Güvenliği",
         "Erişim izni olmayan klasörlerde tarama kesilmez, izinsiz dizinler sessizce geçilir.",
         C_ACCENT2),
        ("4", "Silme Onayı",
         "Kullanıcı bir dosyayı silmeden önce çift katmanlı onay mekanizması devreye girer.",
         C_ACCENT4),
    ]

    for i, (num, title, body, clr) in enumerate(sec_items):
        col = i % 2
        row = i // 2
        x = Inches(0.6) + col * Inches(6.2)
        y = Inches(2.2) + row * Inches(2.15)
        
        # Kart
        card = s6.shapes.add_shape(
            MSO_SHAPE.ROUNDED_RECTANGLE, x, y, Inches(5.8), Inches(1.9)
        )
        card.fill.solid()
        card.fill.fore_color.rgb = C_CARD
        card.line.color.rgb = C_LINE
        card.line.width = Pt(1)
        card.adjustments[0] = 0.05

        # Sol accent cubuk
        bar = s6.shapes.add_shape(
            MSO_SHAPE.RECTANGLE, x, y, Inches(0.08), Inches(1.9)
        )
        bar.fill.solid()
        bar.fill.fore_color.rgb = clr
        bar.line.fill.background()

        # Numara badge
        add_number_badge(s6, x + Inches(0.3), y + Inches(0.35), num, clr)

        # Baslik
        add_text_box(s6, x + Inches(1.0), y + Inches(0.25),
                     Inches(4.5), Inches(0.4), title, font_size=17, color=C_WHITE, bold=True)
        # Icerik
        add_text_box(s6, x + Inches(1.0), y + Inches(0.7),
                     Inches(4.5), Inches(1.0), body, font_size=13, color=C_GRAY)

    add_bottom_strip(s6)

    # ================================================================
    #  SLAYT 7 — AI ÖNERİ SİSTEMİ
    # ================================================================
    s7 = prs.slides.add_slide(blank_layout)
    dark_bg(s7)
    _add_slide_transition(s7, 'cover')
    add_accent_bar(s7, C_ACCENT4)
    add_logo_corner(s7)
    decor_circles(s7)
    page_header(s7, "AI Destekli Akıllı Öneri Sistemi", "Disk temizliğinde yapay zeka devrimi", C_ACCENT4)

    # Sol: Aciklama
    bullets7 = [
        "Dosya yaşını, boyutunu, türünü ve konumunu birlikte değerlendiren çok faktörlü puanlama algoritması.",
        "Geçici dosyalar (temp), log dosyaları, eski güncelleme artıkları ve önbellek klasörleri otomatik tespit edilir.",
        "Her dosyaya dinamik bir risk/önem skoru atanır — yüksek skorlular öncelikli temizlik adayıdır.",
        "Kullanıcı tek tıklama ile gigabaytlarca alanı güvenle geri kazanabilir.",
        "Sistem dosyaları ve kritik uygulamalar asla öneri listesine dahil edilmez."
    ]
    add_bullet_card(s7, Inches(0.6), Inches(2.1), Inches(8.0), Inches(4.3), bullets7, C_ACCENT4)

    # Sag: Gorsel kutu
    ai_box = s7.shapes.add_shape(
        MSO_SHAPE.ROUNDED_RECTANGLE, Inches(9.0), Inches(2.1), Inches(3.7), Inches(4.3)
    )
    ai_box.fill.solid()
    ai_box.fill.fore_color.rgb = C_CARD
    ai_box.line.color.rgb = C_ACCENT4
    ai_box.line.width = Pt(1.5)
    ai_box.adjustments[0] = 0.05

    # AI gorsel: skor cizimi benzeri
    scores = [
        ("temp_cache.dat", "92", C_ACCENT5),
        ("old_update.cab", "87", C_ACCENT5),
        ("game_logs.txt", "78", C_ACCENT),
        ("project_v2.zip", "34", C_ACCENT3),
        ("system32.dll", "3", C_ACCENT3),
    ]
    add_text_box(s7, Inches(9.3), Inches(2.3), Inches(3.2), Inches(0.4),
                 "AI Skor Önizleme", font_size=14, color=C_ACCENT4, bold=True,
                 alignment=PP_ALIGN.CENTER)
    
    for i, (fname, score, clr) in enumerate(scores):
        y = Inches(2.8) + i * Inches(0.65)
        add_text_box(s7, Inches(9.3), y, Inches(1.9), Inches(0.4),
                     fname, font_size=11, color=C_GRAY)
        
        badge = s7.shapes.add_shape(MSO_SHAPE.ROUNDED_RECTANGLE,
                                     Inches(11.5), y + Inches(0.05), Inches(0.85), Inches(0.35))
        badge.fill.solid()
        badge.fill.fore_color.rgb = clr
        badge.line.fill.background()
        badge.adjustments[0] = 0.3
        tf = badge.text_frame
        tf.paragraphs[0].text = score
        tf.paragraphs[0].font.size = Pt(13)
        tf.paragraphs[0].font.color.rgb = C_WHITE
        tf.paragraphs[0].font.bold = True
        tf.paragraphs[0].alignment = PP_ALIGN.CENTER
        tf.vertical_anchor = MSO_ANCHOR.MIDDLE

    add_bottom_strip(s7)

    # ================================================================
    #  SLAYT 8 — MİMARİ ŞEMA
    # ================================================================
    s8 = prs.slides.add_slide(blank_layout)
    dark_bg(s8)
    _add_slide_transition(s8, 'wipe')
    add_accent_bar(s8, C_ACCENT)
    add_logo_corner(s8)
    page_header(s8, "Sistem Mimarisi", "Katmanlı yazılım bileşenleri", C_ACCENT)

    layers = [
        ("Kullanıcı Arayüzü (UI)", "WinForms  |  Karanlık/Aydınlık Tema  |  Modern Kontroller", C_ACCENT2),
        ("Analiz Motoru", "AI Skor Hesaplama  |  Pasta/Treemap Grafik  |  Gerçek Zamanlı İstatistik", C_ACCENT4),
        ("Tarama Katmanı", "NTFS MFT Turbo  |  WinAPI FastScan  |  Paralel I/O  |  Hard-Link Deduplikasyon", C_ACCENT),
        ("Güvenlik Katmanı", "Sistem Koruması  |  Kilitli Dosya Tespiti  |  Yetki Yönetimi", C_ACCENT5),
        ("İşletim Sistemi", "Windows Kernel  |  NTFS  |  Win32 API  |  P/Invoke", C_ACCENT3),
    ]
    for i, (title, desc, clr) in enumerate(layers):
        y = Inches(1.9) + i * Inches(0.95)
        w = Inches(11.0) - i * Inches(0.4)
        x = Inches(0.7) + i * Inches(0.2)
        
        layer_shape = s8.shapes.add_shape(MSO_SHAPE.ROUNDED_RECTANGLE, x, y, w, Inches(0.75))
        layer_shape.fill.solid()
        layer_shape.fill.fore_color.rgb = C_CARD
        layer_shape.line.color.rgb = clr
        layer_shape.line.width = Pt(1.5)
        layer_shape.adjustments[0] = 0.15

        # Sol accent
        dot = s8.shapes.add_shape(MSO_SHAPE.OVAL, x + Inches(0.2), y + Inches(0.18), Inches(0.4), Inches(0.4))
        dot.fill.solid()
        dot.fill.fore_color.rgb = clr
        dot.line.fill.background()
        dtf = dot.text_frame
        dtf.paragraphs[0].text = str(i + 1)
        dtf.paragraphs[0].font.size = Pt(14)
        dtf.paragraphs[0].font.color.rgb = C_BG
        dtf.paragraphs[0].font.bold = True
        dtf.paragraphs[0].alignment = PP_ALIGN.CENTER
        dtf.vertical_anchor = MSO_ANCHOR.MIDDLE

        add_text_box(s8, x + Inches(0.8), y + Inches(0.08), Inches(3.5), Inches(0.35),
                     title, font_size=15, color=C_WHITE, bold=True)
        add_text_box(s8, x + Inches(0.8), y + Inches(0.40), w - Inches(1.2), Inches(0.30),
                     desc, font_size=11, color=C_DIM)

    add_bottom_strip(s8)

    # ================================================================
    #  SLAYT 9 — KAPANIŞ
    # ================================================================
    s9 = prs.slides.add_slide(blank_layout)
    dark_bg(s9)
    _add_slide_transition(s9, 'fade', 'slow')

    # Dekoratif arka plan
    big_c2 = s9.shapes.add_shape(MSO_SHAPE.OVAL, Inches(3.5), Inches(-2), Inches(12), Inches(12))
    big_c2.fill.solid()
    big_c2.fill.fore_color.rgb = RGBColor(22, 22, 28)
    big_c2.line.color.rgb = RGBColor(35, 35, 45)
    big_c2.line.width = Pt(2)

    add_accent_bar(s9, C_ACCENT, Inches(0.15))

    # Logo
    if logo_path:
        s9.shapes.add_picture(logo_path, (EMU_SLIDE_W - Inches(2.2)) // 2, Inches(1.5), width=Inches(2.2))

    add_text_box(s9, Inches(0), Inches(3.8), EMU_SLIDE_W, Inches(0.8),
                 "Teşekkürler", font_size=48, color=C_WHITE, bold=True,
                 font_name="Segoe UI Semibold", alignment=PP_ALIGN.CENTER)

    add_text_box(s9, Inches(0), Inches(4.7), EMU_SLIDE_W, Inches(0.5),
                 "Sorularınız ve geri bildirimleriniz için hazırız", font_size=20,
                 color=C_ACCENT, alignment=PP_ALIGN.CENTER)

    add_text_box(s9, Inches(0), Inches(5.5), EMU_SLIDE_W, Inches(0.4),
                 "Advanced Disk Analyzer — Açık Kaynak · Güvenli · Akıllı · Hızlı",
                 font_size=13, color=C_DIM, alignment=PP_ALIGN.CENTER)

    add_bottom_strip(s9)

    # ── KAYDET ──
    out = "AdvancedDiskAnalyzer_Sunum.pptx"
    prs.save(out)
    print(f"Profesyonel sunum basariyla olusturuldu: {out}")
    print(f"  - {len(prs.slides)} slayt")
    print(f"  - Logo: {logo_path}")

if __name__ == "__main__":
    build()
