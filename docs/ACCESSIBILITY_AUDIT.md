# Audyt dostępności (WCAG 2.2 AA)

Data: 7 lutego 2026

## Zakres
Interfejs webowy (MVC), widoki list, formularze logowania/rejestracji, forum, placówki, opinie, kontakt, panel.

## Sprawdzenia wykonane
### Nawigacja klawiaturą
- Fokus widoczny na elementach interaktywnych
- Skip link do treści głównej
- Menu mobilne z `aria-expanded` i `aria-controls`

### Ruch i animacje
- Respektowanie `prefers-reduced-motion`
- Tryb „Mniej ruchu” wyłącza animacje

### Czytelność
- Zwiększony rozmiar tekstu (tryb)
- Wysoki kontrast (tryb)
- Poprawa widoczności błędów walidacji

### Informacje o stanie
- Komunikaty błędów wyróżnione kolorystycznie i pogrubieniem

## Rekomendacje do weryfikacji manualnej
- Kontrast tekstu/ikon na przyciskach w trybie jasnym/ciemnym
- Opisy alternatywne dla obrazów (w galeriach i treściach)
- Spójność nagłówków (hierarchia H1–H4)
- Etykiety i instrukcje przy formularzach

## Narzędzia
- AXE DevTools
- Lighthouse (Accessibility)
- WAVE

## Wynik
Wdrożono kluczowe usprawnienia dostępności oraz tryby ułatwiające. Zalecane jest wykonanie pełnego audytu manualnego z udziałem użytkowników.
