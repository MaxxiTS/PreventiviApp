import 'package:intl/intl.dart';

/// Utilidades de formateo para la UI (locale es-ES).
class Formato {
  const Formato._();

  static final NumberFormat _moneda = NumberFormat.currency(
    locale: 'es_ES',
    symbol: '€',
    decimalDigits: 2,
  );

  static final NumberFormat _numero = NumberFormat.decimalPattern('es_ES');

  /// Formatea un importe como euros con separadores es-ES, p. ej. `1.234,56 €`.
  static String euros(num valor) => _moneda.format(valor);

  /// Formatea un número con separadores de miles es-ES.
  static String numero(num valor) => _numero.format(valor);

  /// Formatea una fecha ISO (`yyyy-MM-dd` o ISO 8601) a `dd/MM/yyyy`.
  /// Devuelve cadena vacía si la entrada es nula o no parseable.
  static String fecha(String? iso) {
    if (iso == null || iso.isEmpty) return '';
    final parsed = DateTime.tryParse(iso);
    if (parsed == null) return iso;
    return DateFormat('dd/MM/yyyy', 'es_ES').format(parsed);
  }
}
