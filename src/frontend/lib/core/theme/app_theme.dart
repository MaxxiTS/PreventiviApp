import 'package:flutter/material.dart';

/// Tema de la aplicación (Material 3) en variantes clara y oscura.
///
/// Paleta sobria inspirada en Linear/Notion, con azul técnico como color de
/// acento (ver `docs/07-mockups-ux-ui.md` §2.1).
class AppTheme {
  const AppTheme._();

  // --- Color de acento (azul técnico) ---
  static const Color _primaryLight = Color(0xFF2563EB);
  static const Color _primaryDark = Color(0xFF3B82F6);

  // --- Fondos ---
  static const Color _bgBaseLight = Color(0xFFFFFFFF);
  static const Color _bgSubtleLight = Color(0xFFF7F8FA);
  static const Color _bgBaseDark = Color(0xFF16181D);
  static const Color _bgSubtleDark = Color(0xFF0F1115);

  // --- Texto / bordes ---
  static const Color _textPrimaryLight = Color(0xFF1A1D23);
  static const Color _textPrimaryDark = Color(0xFFE8EAED);
  static const Color _borderLight = Color(0xFFE2E5EA);
  static const Color _borderDark = Color(0xFF2A2E37);

  /// Tema claro.
  static ThemeData light() => _build(
        brightness: Brightness.light,
        primary: _primaryLight,
        background: _bgBaseLight,
        surfaceSubtle: _bgSubtleLight,
        onBackground: _textPrimaryLight,
        border: _borderLight,
      );

  /// Tema oscuro.
  static ThemeData dark() => _build(
        brightness: Brightness.dark,
        primary: _primaryDark,
        background: _bgBaseDark,
        surfaceSubtle: _bgSubtleDark,
        onBackground: _textPrimaryDark,
        border: _borderDark,
      );

  static ThemeData _build({
    required Brightness brightness,
    required Color primary,
    required Color background,
    required Color surfaceSubtle,
    required Color onBackground,
    required Color border,
  }) {
    final colorScheme = ColorScheme.fromSeed(
      seedColor: primary,
      brightness: brightness,
    ).copyWith(
      primary: primary,
      surface: background,
    );

    return ThemeData(
      useMaterial3: true,
      brightness: brightness,
      colorScheme: colorScheme,
      scaffoldBackgroundColor: surfaceSubtle,
      visualDensity: VisualDensity.compact,
      cardTheme: CardThemeData(
        elevation: 0,
        color: background,
        clipBehavior: Clip.antiAlias,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(10),
          side: BorderSide(color: border),
        ),
      ),
      appBarTheme: AppBarTheme(
        backgroundColor: surfaceSubtle,
        foregroundColor: onBackground,
        elevation: 0,
        scrolledUnderElevation: 0.5,
        centerTitle: false,
      ),
      navigationRailTheme: NavigationRailThemeData(
        backgroundColor: surfaceSubtle,
        indicatorColor: primary.withValues(alpha: 0.14),
        selectedIconTheme: IconThemeData(color: primary),
        selectedLabelTextStyle: TextStyle(
          color: primary,
          fontWeight: FontWeight.w600,
        ),
        unselectedLabelTextStyle: TextStyle(
          color: onBackground.withValues(alpha: 0.7),
        ),
      ),
      inputDecorationTheme: InputDecorationTheme(
        isDense: true,
        border: OutlineInputBorder(
          borderRadius: BorderRadius.circular(8),
          borderSide: BorderSide(color: border),
        ),
        enabledBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(8),
          borderSide: BorderSide(color: border),
        ),
      ),
      dividerTheme: DividerThemeData(color: border, thickness: 1, space: 1),
      filledButtonTheme: FilledButtonThemeData(
        style: FilledButton.styleFrom(
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(8),
          ),
        ),
      ),
    );
  }
}
