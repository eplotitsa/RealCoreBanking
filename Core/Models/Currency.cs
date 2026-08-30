using RealCoreBanking.Core.Enums;

namespace RealCoreBanking.Core.Models;

public class Currency
{
    public string      AlphaCode    { get; set; } = "";   // ISO 4217 Alpha-3, e.g. "USD"
    public string      NumericCode  { get; set; } = "";   // ISO 4217 Numeric-3, e.g. "840"
    public string      Name         { get; set; } = "";   // max 128 chars
    public int         DecimalPlaces{ get; set; } = 2;    // precision m in (n,m)
    public RoundingRule RoundingRule{ get; set; } = RoundingRule.AwayFromZero;

    public override string ToString() => $"{AlphaCode} — {Name}";

    // ── Seed data (used when DB has no currencies yet) ────────────────────────
    public static IReadOnlyList<Currency> SeedList { get; } = new List<Currency>
    {
        // Major reserve currencies
        new() { AlphaCode="USD", NumericCode="840", Name="US Dollar",                   DecimalPlaces=2 },
        new() { AlphaCode="EUR", NumericCode="978", Name="Euro",                        DecimalPlaces=2 },
        new() { AlphaCode="GBP", NumericCode="826", Name="Pound Sterling",              DecimalPlaces=2 },
        new() { AlphaCode="CHF", NumericCode="756", Name="Swiss Franc",                 DecimalPlaces=2 },
        new() { AlphaCode="JPY", NumericCode="392", Name="Japanese Yen",                DecimalPlaces=0 },
        new() { AlphaCode="CNY", NumericCode="156", Name="Chinese Yuan Renminbi",       DecimalPlaces=2 },
        // Asia-Pacific
        new() { AlphaCode="AUD", NumericCode="036", Name="Australian Dollar",           DecimalPlaces=2 },
        new() { AlphaCode="NZD", NumericCode="554", Name="New Zealand Dollar",          DecimalPlaces=2 },
        new() { AlphaCode="HKD", NumericCode="344", Name="Hong Kong Dollar",            DecimalPlaces=2 },
        new() { AlphaCode="SGD", NumericCode="702", Name="Singapore Dollar",            DecimalPlaces=2 },
        new() { AlphaCode="KRW", NumericCode="410", Name="South Korean Won",            DecimalPlaces=0 },
        new() { AlphaCode="TWD", NumericCode="901", Name="New Taiwan Dollar",           DecimalPlaces=2 },
        new() { AlphaCode="THB", NumericCode="764", Name="Thai Baht",                   DecimalPlaces=2 },
        new() { AlphaCode="MYR", NumericCode="458", Name="Malaysian Ringgit",           DecimalPlaces=2 },
        new() { AlphaCode="IDR", NumericCode="360", Name="Indonesian Rupiah",           DecimalPlaces=2 },
        new() { AlphaCode="PHP", NumericCode="608", Name="Philippine Peso",             DecimalPlaces=2 },
        new() { AlphaCode="VND", NumericCode="704", Name="Vietnamese Dong",             DecimalPlaces=0 },
        new() { AlphaCode="INR", NumericCode="356", Name="Indian Rupee",                DecimalPlaces=2 },
        new() { AlphaCode="PKR", NumericCode="586", Name="Pakistani Rupee",             DecimalPlaces=2 },
        new() { AlphaCode="BDT", NumericCode="050", Name="Bangladeshi Taka",            DecimalPlaces=2 },
        new() { AlphaCode="LKR", NumericCode="144", Name="Sri Lankan Rupee",            DecimalPlaces=2 },
        new() { AlphaCode="NPR", NumericCode="524", Name="Nepalese Rupee",              DecimalPlaces=2 },
        new() { AlphaCode="MNT", NumericCode="496", Name="Mongolian Tugrik",            DecimalPlaces=2 },
        // North America
        new() { AlphaCode="CAD", NumericCode="124", Name="Canadian Dollar",             DecimalPlaces=2 },
        new() { AlphaCode="MXN", NumericCode="484", Name="Mexican Peso",                DecimalPlaces=2 },
        // Latin America
        new() { AlphaCode="BRL", NumericCode="986", Name="Brazilian Real",              DecimalPlaces=2 },
        new() { AlphaCode="ARS", NumericCode="032", Name="Argentine Peso",              DecimalPlaces=2 },
        new() { AlphaCode="CLP", NumericCode="152", Name="Chilean Peso",                DecimalPlaces=0 },
        new() { AlphaCode="COP", NumericCode="170", Name="Colombian Peso",              DecimalPlaces=2 },
        new() { AlphaCode="PEN", NumericCode="604", Name="Peruvian Sol",                DecimalPlaces=2 },
        new() { AlphaCode="UYU", NumericCode="858", Name="Uruguayan Peso",              DecimalPlaces=2 },
        // Middle East & Gulf
        new() { AlphaCode="AED", NumericCode="784", Name="UAE Dirham",                  DecimalPlaces=2 },
        new() { AlphaCode="SAR", NumericCode="682", Name="Saudi Riyal",                 DecimalPlaces=2 },
        new() { AlphaCode="QAR", NumericCode="634", Name="Qatari Riyal",                DecimalPlaces=2 },
        new() { AlphaCode="KWD", NumericCode="414", Name="Kuwaiti Dinar",               DecimalPlaces=3 },
        new() { AlphaCode="BHD", NumericCode="048", Name="Bahraini Dinar",              DecimalPlaces=3 },
        new() { AlphaCode="OMR", NumericCode="512", Name="Omani Rial",                  DecimalPlaces=3 },
        new() { AlphaCode="JOD", NumericCode="400", Name="Jordanian Dinar",             DecimalPlaces=3 },
        new() { AlphaCode="ILS", NumericCode="376", Name="Israeli New Shekel",          DecimalPlaces=2 },
        new() { AlphaCode="TRY", NumericCode="949", Name="Turkish Lira",                DecimalPlaces=2 },
        new() { AlphaCode="IQD", NumericCode="368", Name="Iraqi Dinar",                 DecimalPlaces=3 },
        // Europe (non-EUR)
        new() { AlphaCode="NOK", NumericCode="578", Name="Norwegian Krone",             DecimalPlaces=2 },
        new() { AlphaCode="SEK", NumericCode="752", Name="Swedish Krona",               DecimalPlaces=2 },
        new() { AlphaCode="DKK", NumericCode="208", Name="Danish Krone",                DecimalPlaces=2 },
        new() { AlphaCode="PLN", NumericCode="985", Name="Polish Zloty",                DecimalPlaces=2 },
        new() { AlphaCode="CZK", NumericCode="203", Name="Czech Koruna",                DecimalPlaces=2 },
        new() { AlphaCode="HUF", NumericCode="348", Name="Hungarian Forint",            DecimalPlaces=2 },
        new() { AlphaCode="RON", NumericCode="946", Name="Romanian Leu",                DecimalPlaces=2 },
        new() { AlphaCode="BGN", NumericCode="975", Name="Bulgarian Lev",               DecimalPlaces=2 },
        new() { AlphaCode="HRK", NumericCode="191", Name="Croatian Kuna",               DecimalPlaces=2 },
        new() { AlphaCode="RSD", NumericCode="941", Name="Serbian Dinar",               DecimalPlaces=2 },
        new() { AlphaCode="ALL", NumericCode="008", Name="Albanian Lek",                DecimalPlaces=2 },
        new() { AlphaCode="MKD", NumericCode="807", Name="Macedonian Denar",            DecimalPlaces=2 },
        new() { AlphaCode="BAM", NumericCode="977", Name="Bosnia-Herzegovina Conv. Mark",DecimalPlaces=2},
        // CIS / Eastern Europe
        new() { AlphaCode="RUB", NumericCode="643", Name="Russian Ruble",               DecimalPlaces=2 },
        new() { AlphaCode="UAH", NumericCode="980", Name="Ukrainian Hryvnia",           DecimalPlaces=2 },
        new() { AlphaCode="KZT", NumericCode="398", Name="Kazakhstani Tenge",           DecimalPlaces=2 },
        new() { AlphaCode="BYN", NumericCode="933", Name="Belarusian Ruble",            DecimalPlaces=2 },
        new() { AlphaCode="UZS", NumericCode="860", Name="Uzbekistani Som",             DecimalPlaces=2 },
        new() { AlphaCode="GEL", NumericCode="981", Name="Georgian Lari",               DecimalPlaces=2 },
        new() { AlphaCode="AMD", NumericCode="051", Name="Armenian Dram",               DecimalPlaces=2 },
        new() { AlphaCode="AZN", NumericCode="944", Name="Azerbaijani Manat",           DecimalPlaces=2 },
        new() { AlphaCode="MDL", NumericCode="498", Name="Moldovan Leu",                DecimalPlaces=2 },
        new() { AlphaCode="KGS", NumericCode="417", Name="Kyrgyzstani Som",             DecimalPlaces=2 },
        new() { AlphaCode="TJS", NumericCode="972", Name="Tajikistani Somoni",          DecimalPlaces=2 },
        new() { AlphaCode="TMT", NumericCode="934", Name="Turkmenistan Manat",          DecimalPlaces=2 },
        // Africa
        new() { AlphaCode="ZAR", NumericCode="710", Name="South African Rand",          DecimalPlaces=2 },
        new() { AlphaCode="NGN", NumericCode="566", Name="Nigerian Naira",              DecimalPlaces=2 },
        new() { AlphaCode="EGP", NumericCode="818", Name="Egyptian Pound",              DecimalPlaces=2 },
        new() { AlphaCode="KES", NumericCode="404", Name="Kenyan Shilling",             DecimalPlaces=2 },
        new() { AlphaCode="GHS", NumericCode="936", Name="Ghanaian Cedi",               DecimalPlaces=2 },
        new() { AlphaCode="TZS", NumericCode="834", Name="Tanzanian Shilling",          DecimalPlaces=2 },
        new() { AlphaCode="ETB", NumericCode="230", Name="Ethiopian Birr",              DecimalPlaces=2 },
        new() { AlphaCode="MAD", NumericCode="504", Name="Moroccan Dirham",             DecimalPlaces=2 },
        new() { AlphaCode="TND", NumericCode="788", Name="Tunisian Dinar",              DecimalPlaces=3 },
        new() { AlphaCode="DZD", NumericCode="012", Name="Algerian Dinar",              DecimalPlaces=2 },
        new() { AlphaCode="XOF", NumericCode="952", Name="West African CFA Franc",      DecimalPlaces=0 },
        new() { AlphaCode="XAF", NumericCode="950", Name="Central African CFA Franc",   DecimalPlaces=0 },
        new() { AlphaCode="AOA", NumericCode="973", Name="Angolan Kwanza",              DecimalPlaces=2 },
        new() { AlphaCode="ZMW", NumericCode="967", Name="Zambian Kwacha",              DecimalPlaces=2 },
        new() { AlphaCode="MZN", NumericCode="943", Name="Mozambican Metical",          DecimalPlaces=2 },
        new() { AlphaCode="BWP", NumericCode="072", Name="Botswanan Pula",              DecimalPlaces=2 },
    }.AsReadOnly();

    // ── Static lookup (works before AccountStore is available) ────────────────
    public static Currency? GetByNumericCode(string numericCode) =>
        SeedList.FirstOrDefault(c => c.NumericCode == numericCode);

    public static Currency? GetByAlphaCode(string alphaCode) =>
        SeedList.FirstOrDefault(c =>
            c.AlphaCode.Equals(alphaCode, StringComparison.OrdinalIgnoreCase));
}
