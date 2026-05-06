import React from 'react';

type Props = {
  value: string;
  onChange: (html: string) => void;
  minHeight?: number;
};

export interface RichTextLiteRef {
  insertText: (text: string) => void;
}

function applyInlineStyle(style: string, value: string) {
  const sel = window.getSelection?.();
  if (!sel || sel.rangeCount === 0) return;

  const range = sel.getRangeAt(0);
  if (range.collapsed) {
    // inserta span vacío y deja el cursor adentro
    const span = document.createElement('span');
    span.style.setProperty(style, value);
    span.appendChild(document.createTextNode('\u200b')); // zero-width space
    range.insertNode(span);
    sel.removeAllRanges();
    const newRange = document.createRange();
    newRange.setStart(span.firstChild as Text, 1);
    newRange.collapse(true);
    sel.addRange(newRange);
    return;
  }

  // envolver selección en un span con el estilo
  const span = document.createElement('span');
  span.style.setProperty(style, value);
  try {
    span.appendChild(range.extractContents());
    range.insertNode(span);
    // re-seleccionar el contenido insertado
    const newRange = document.createRange();
    newRange.selectNodeContents(span);
    sel.removeAllRanges();
    sel.addRange(newRange);
  } catch {
    // fallback si la selección cruza nodos incompatibles
    document.execCommand('styleWithCSS', false, 'true');
    document.execCommand(style === 'font-size' ? 'fontSize' : 'insertText', false, '');
  }
}

const RichTextLite = React.forwardRef<RichTextLiteRef, Props>(({ value, onChange, minHeight = 160 }, forwardedRef) => {
  const ref = React.useRef<HTMLDivElement | null>(null);
  const isSyncingRef = React.useRef(false);

  // sync value -> editor
  React.useEffect(() => {
    const el = ref.current;
    if (!el) return;
    if (isSyncingRef.current) return;
    if (el.innerHTML !== value) el.innerHTML = value || '';
  }, [value]);

  // change handler
  const handleInput = React.useCallback(() => {
    const el = ref.current;
    if (!el) return;
    isSyncingRef.current = true;
    onChange(el.innerHTML);
    // liberar flag después del microtask
    queueMicrotask(() => { isSyncingRef.current = false; });
  }, [onChange]);

  // toolbar actions
  const cmd = (command: string, value?: string) => {
    document.execCommand('styleWithCSS', false, 'true'); // fuerza estilos inline
    document.execCommand(command, false, value ?? '');
    handleInput();
  };

  const setFontSize = (px: string) => {
    applyInlineStyle('font-size', px);
    handleInput();
  };

  const setFontFamily = (font: string) => {
    document.execCommand('styleWithCSS', false, 'true');
    document.execCommand('fontName', false, font);
    handleInput();
  };

  const setColor = (color: string) => {
    document.execCommand('styleWithCSS', false, 'true');
    document.execCommand('foreColor', false, color);
    handleInput();
  };

  // Expose insertText method via ref
  React.useImperativeHandle(forwardedRef, () => ({
    insertText: (text: string) => {
      const el = ref.current;
      if (!el) return;

      // Focus the editor first
      el.focus();

      // Get current selection
      const sel = window.getSelection();
      if (!sel || sel.rangeCount === 0) {
        // No selection, append at the end
        el.innerHTML += text;
      } else {
        // Insert at cursor position
        const range = sel.getRangeAt(0);
        range.deleteContents();
        const textNode = document.createTextNode(text);
        range.insertNode(textNode);

        // Move cursor after inserted text
        range.setStartAfter(textNode);
        range.collapse(true);
        sel.removeAllRanges();
        sel.addRange(range);
      }

      handleInput();
    }
  }), [handleInput]);

  return (
    <div style={{ border: '1px solid #ccc', borderRadius: 6 }}>
      {/* Toolbar */}
      <div
        style={{
          display: 'flex',
          gap: 8,
          flexWrap: 'wrap',
          padding: 8,
          borderBottom: '1px solid #ddd',
          alignItems: 'center'
        }}
      >
        <button type="button" onClick={() => cmd('bold')}>B</button>
        <button type="button" onClick={() => cmd('italic')}><em>I</em></button>
        <button type="button" onClick={() => cmd('underline')}><u>U</u></button>
        <span style={{ margin: '0 8px', opacity: .6 }}>|</span>
        <button type="button" onClick={() => cmd('justifyLeft')}>Izq</button>
        <button type="button" onClick={() => cmd('justifyCenter')}>Centro</button>
        <button type="button" onClick={() => cmd('justifyRight')}>Der</button>
        <button type="button" onClick={() => cmd('justifyFull')}>Just</button>
        <span style={{ margin: '0 8px', opacity: .6 }}>|</span>
        <button type="button" onClick={() => cmd('insertUnorderedList')}>• Lista</button>
        <button type="button" onClick={() => cmd('insertOrderedList')}>1. Lista</button>
        <span style={{ margin: '0 8px', opacity: .6 }}>|</span>
        <select
          defaultValue=""
          onChange={(e) => { if (e.target.value) setFontFamily(e.target.value); e.currentTarget.value = ''; }}
          style={{ padding: 4 }}
        >
          <option value="" disabled>Fuente</option>
          <option value="Arial">Arial</option>
          <option value="Helvetica">Helvetica</option>
          <option value="Times New Roman">Times New Roman</option>
          <option value="Georgia">Georgia</option>
          <option value="Verdana">Verdana</option>
          <option value="Courier New">Courier New</option>
          <option value="Trebuchet MS">Trebuchet MS</option>
          <option value="Comic Sans MS">Comic Sans MS</option>
        </select>
        <select
          defaultValue=""
          onChange={(e) => { if (e.target.value) setFontSize(e.target.value); e.currentTarget.value = ''; }}
          style={{ padding: 4 }}
        >
          <option value="" disabled>Tamaño</option>
          <option value="12px">12 px</option>
          <option value="14px">14 px</option>
          <option value="16px">16 px</option>
          <option value="18px">18 px</option>
          <option value="20px">20 px</option>
          <option value="22px">22 px</option>
          <option value="24px">24 px</option>
        </select>
        <select
          defaultValue=""
          onChange={(e) => { if (e.target.value) setColor(e.target.value); e.currentTarget.value = ''; }}
          style={{ padding: 4 }}
        >
          <option value="" disabled>Color</option>
          <option value="#000000">Negro</option>
          <option value="#FFFFFF">Blanco</option>
          <option value="#FF0000">Rojo</option>
          <option value="#0000FF">Azul</option>
          <option value="#008000">Verde</option>
          <option value="#FFA500">Naranja</option>
          <option value="#800080">Púrpura</option>
          <option value="#808080">Gris</option>
        </select>
        <button type="button" onClick={() => cmd('removeFormat')}>Limpiar</button>
      </div>

      {/* Editable */}
      <div
        ref={ref}
        contentEditable
        onInput={handleInput}
        onBlur={handleInput}
        style={{
          minHeight,
          padding: 12,
          outline: 'none',
          lineHeight: 1.4,
          whiteSpace: 'pre-wrap'
        }}
        // Impedimos que Enter meta <div> anidados raros en algunos navegadores
        onKeyDown={(e) => {
          if (e.key === 'Tab') { e.preventDefault(); document.execCommand('insertText', false, '    '); }
        }}
        suppressContentEditableWarning
      />
    </div>
  );
});

RichTextLite.displayName = 'RichTextLite';

export default RichTextLite;
