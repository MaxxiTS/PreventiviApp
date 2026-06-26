// Preventivi App — punto de entrada Flutter (esqueleto)
// La estructura feature-first y la configuración real (router, tema dark/light,
// inyección de dependencias con Riverpod, BD local) se desarrollarán siguiendo
// docs/04-estructura-carpetas.md y docs/07-mockups-ux-ui.md.

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

void main() {
  runApp(const ProviderScope(child: PreventiviApp()));
}

class PreventiviApp extends StatelessWidget {
  const PreventiviApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Preventivi App',
      theme: ThemeData.light(useMaterial3: true),
      darkTheme: ThemeData.dark(useMaterial3: true),
      themeMode: ThemeMode.system,
      home: const Scaffold(
        body: Center(child: Text('Preventivi App — fase de diseño')),
      ),
    );
  }
}
