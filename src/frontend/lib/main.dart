// Preventivi App — punto de entrada Flutter.
//
// Estructura feature-first (data/domain/presentation) con Riverpod (estado),
// go_router (navegación) y dio (API REST .NET). Offline-first; la persistencia
// local (Drift) se añadirá en un incremento posterior.

import 'package:flutter/material.dart';
import 'package:flutter_localizations/flutter_localizations.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:intl/date_symbol_data_local.dart';

import 'core/config/app_config.dart';
import 'core/router/app_router.dart';
import 'core/theme/app_theme.dart';

Future<void> main() async {
  WidgetsFlutterBinding.ensureInitialized();
  // Inicializa los datos de formato de fecha/número para es-ES.
  await initializeDateFormatting('es_ES');
  runApp(const ProviderScope(child: PreventiviApp()));
}

class PreventiviApp extends ConsumerWidget {
  const PreventiviApp({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final router = ref.watch(routerProvider);

    return MaterialApp.router(
      title: AppConfig.appName,
      debugShowCheckedModeBanner: false,
      theme: AppTheme.light(),
      darkTheme: AppTheme.dark(),
      themeMode: ThemeMode.system,
      routerConfig: router,
      // Localización: español como idioma principal.
      locale: const Locale('es', 'ES'),
      supportedLocales: const [Locale('es', 'ES'), Locale('en')],
      localizationsDelegates: const [
        GlobalMaterialLocalizations.delegate,
        GlobalWidgetsLocalizations.delegate,
        GlobalCupertinoLocalizations.delegate,
      ],
    );
  }
}
