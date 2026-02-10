# Raport Produkcji - System Raportowania Produkcji ISA-95

System raportowania produkcji oparty na standardzie ISA-95, z prostym interfejsem kafelkowym do zarządzania procesem produkcyjnym.

## Funkcjonalności

### 1. Planowanie
- Planowanie zleceń produkcyjnych na zasyp
- Definiowanie nazwy produktu i numeru receptury
- Wybór typu worka:
  - Worki zgrzewane 20kg
  - Worki zgrzewane 25kg
  - Worki szyte 20kg
  - Worki szyte 25kg
  - BigBag
- Określanie planowanej wagi produkcji
- Automatyczne obliczanie czasu workowania

### 2. Zasyp (Produkcja)
- Wymieszanie surowców
- Dodawanie szarż z rzeczywistą wagą
- Automatyczne zliczanie liczby szarż
- Śledzenie postępu realizacji zlecenia
- Historia wszystkich szarż z operatorami i datami

### 3. Workowanie
- Automatyczne przekazywanie zleceń gotowych do workowania
- Wyświetlanie szacowanego czasu workowania na podstawie typu worka:
  - Worki zgrzewane: 1000kg / 20 min
  - Worki szyte: 1000kg / 30 min
  - BigBag: 1000kg / 15 min
- Zarządzanie statusem workowania

## Technologie

- .NET 10.0
- ASP.NET Core Blazor (Interactive Server)
- Bootstrap 5
- Bootstrap Icons

## Uruchomienie

```bash
cd src/RaportProdukcji
dotnet restore
dotnet run
```

Aplikacja będzie dostępna pod adresem: `http://localhost:5000`

## Struktura Projektu

```
src/RaportProdukcji/
├── Models/              # Modele domenowe (ProductionOrder, Batch, BagType)
├── Services/            # Warstwa serwisowa (ProductionService)
├── Components/
│   ├── Pages/          # Strony Blazor (Home, Planning, Filling, Bagging)
│   └── Layout/         # Komponenty layoutu
└── wwwroot/            # Zasoby statyczne
```

## Standard ISA-95

Aplikacja implementuje podstawowe koncepcje standardu ISA-95:
- Zarządzanie zleceniami produkcyjnymi
- Śledzenie materiałów i szarż
- Raportowanie produkcji
- Zarządzanie statusem zleceń

## Przepływ Pracy

1. **Planista** tworzy zlecenie produkcyjne określając produkt, recepturę, typ worka i planowaną wagę
2. **Operator zasypu** dodaje kolejne szarże, system automatycznie zlicza ilość i wagę
3. Po osiągnięciu planowanej wagi, zlecenie automatycznie przechodzi do statusu "Gotowe do workowania"
4. **Operator workowania** może rozpocząć i zakończyć proces pakowania