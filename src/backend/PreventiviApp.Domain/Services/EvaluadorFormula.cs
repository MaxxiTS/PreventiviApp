using System.Globalization;

namespace PreventiviApp.Domain.Services;

/// <summary>
/// Evaluador de fórmulas mediante un analizador descendente recursivo. Calcula en
/// <see cref="decimal"/> para la aritmética y delega en <see cref="double"/> solo
/// las funciones trascendentes (sqrt, pow, sin, cos), preservando la precisión
/// monetaria en el caso común.
///
/// Gramática:
///   expr    := term   (('+' | '-') term)*
///   term    := factor (('*' | '/') factor)*
///   factor  := ('+' | '-') factor | primary
///   primary := numero | ident | ident '(' args? ')' | '(' expr ')'
///   args    := expr (',' expr)*
/// </summary>
public sealed class EvaluadorFormula : IEvaluadorFormula
{
    private const decimal Pi = 3.14159265358979m;

    public decimal Evaluar(string formula, IReadOnlyDictionary<string, decimal> variables)
    {
        if (string.IsNullOrWhiteSpace(formula))
            throw new FormatException("La fórmula está vacía.");

        var parser = new Parser(formula, variables);
        var resultado = parser.ParseExpr();
        parser.EsperarFin();
        return resultado;
    }

    private sealed class Parser(string texto, IReadOnlyDictionary<string, decimal> variables)
    {
        private readonly string _texto = texto;
        private readonly IReadOnlyDictionary<string, decimal> _vars = variables;
        private int _pos = 0;

        public decimal ParseExpr()
        {
            var valor = ParseTerm();
            while (true)
            {
                char? op = OjearOperador('+', '-');
                if (op is null) return valor;
                Avanzar();
                var derecha = ParseTerm();
                valor = op == '+' ? valor + derecha : valor - derecha;
            }
        }

        private decimal ParseTerm()
        {
            var valor = ParseFactor();
            while (true)
            {
                char? op = OjearOperador('*', '/');
                if (op is null) return valor;
                Avanzar();
                var derecha = ParseFactor();
                if (op == '/')
                {
                    if (derecha == 0m) throw new FormatException("División por cero en la fórmula.");
                    valor /= derecha;
                }
                else
                {
                    valor *= derecha;
                }
            }
        }

        private decimal ParseFactor()
        {
            SaltarEspacios();
            char c = Actual();
            if (c == '+') { Avanzar(); return ParseFactor(); }
            if (c == '-') { Avanzar(); return -ParseFactor(); }
            return ParsePrimary();
        }

        private decimal ParsePrimary()
        {
            SaltarEspacios();
            char c = Actual();

            if (c == '(')
            {
                Avanzar();
                var valor = ParseExpr();
                Esperar(')');
                return valor;
            }

            if (char.IsDigit(c) || c == '.')
                return ParseNumero();

            if (char.IsLetter(c) || c == '_')
                return ParseIdentificador();

            throw new FormatException($"Carácter inesperado '{c}' en la posición {_pos}.");
        }

        private decimal ParseNumero()
        {
            int inicio = _pos;
            while (!Fin() && (char.IsDigit(Actual()) || Actual() == '.')) _pos++;
            var lexema = _texto[inicio.._pos];
            if (!decimal.TryParse(lexema, NumberStyles.Number, CultureInfo.InvariantCulture, out var numero))
                throw new FormatException($"Número inválido '{lexema}'.");
            return numero;
        }

        private decimal ParseIdentificador()
        {
            int inicio = _pos;
            while (!Fin() && (char.IsLetterOrDigit(Actual()) || Actual() == '_')) _pos++;
            var nombre = _texto[inicio.._pos];

            SaltarEspacios();
            if (!Fin() && Actual() == '(')
            {
                Avanzar();
                var args = ParseArgs();
                Esperar(')');
                return AplicarFuncion(nombre, args);
            }

            // Constante o variable.
            if (string.Equals(nombre, "PI", StringComparison.OrdinalIgnoreCase))
                return Pi;
            if (_vars.TryGetValue(nombre, out var valorVar))
                return valorVar;

            throw new FormatException($"Variable desconocida '{nombre}'.");
        }

        private List<decimal> ParseArgs()
        {
            var args = new List<decimal>();
            SaltarEspacios();
            if (!Fin() && Actual() == ')') return args; // sin argumentos
            args.Add(ParseExpr());
            while (true)
            {
                SaltarEspacios();
                if (Fin() || Actual() != ',') return args;
                Avanzar();
                args.Add(ParseExpr());
            }
        }

        private static decimal AplicarFuncion(string nombre, List<decimal> a)
        {
            decimal Uno() => a.Count == 1 ? a[0] : throw new FormatException($"{nombre} requiere 1 argumento.");
            (decimal, decimal) Dos() => a.Count == 2 ? (a[0], a[1]) : throw new FormatException($"{nombre} requiere 2 argumentos.");

            switch (nombre.ToLowerInvariant())
            {
                case "sqrt": return (decimal)Math.Sqrt((double)Uno());
                case "abs": return Math.Abs(Uno());
                case "sin": return (decimal)Math.Sin((double)Uno());
                case "cos": return (decimal)Math.Cos((double)Uno());
                case "pow": { var (x, y) = Dos(); return (decimal)Math.Pow((double)x, (double)y); }
                case "min": { var (x, y) = Dos(); return Math.Min(x, y); }
                case "max": { var (x, y) = Dos(); return Math.Max(x, y); }
                case "round":
                    if (a.Count == 1) return Math.Round(a[0], MidpointRounding.AwayFromZero);
                    if (a.Count == 2) return Math.Round(a[0], (int)a[1], MidpointRounding.AwayFromZero);
                    throw new FormatException("round requiere 1 o 2 argumentos.");
                default:
                    throw new FormatException($"Función desconocida '{nombre}'.");
            }
        }

        public void EsperarFin()
        {
            SaltarEspacios();
            if (!Fin())
                throw new FormatException($"Texto sobrante en la fórmula en la posición {_pos}.");
        }

        // --- utilidades de lectura ---
        private bool Fin() => _pos >= _texto.Length;
        private char Actual() => Fin() ? '\0' : _texto[_pos];
        private void Avanzar() => _pos++;
        private void SaltarEspacios() { while (!Fin() && char.IsWhiteSpace(Actual())) _pos++; }

        private char? OjearOperador(char a, char b)
        {
            SaltarEspacios();
            char c = Actual();
            return c == a || c == b ? c : null;
        }

        private void Esperar(char c)
        {
            SaltarEspacios();
            if (Fin() || Actual() != c)
                throw new FormatException($"Se esperaba '{c}' en la posición {_pos}.");
            Avanzar();
        }
    }
}
