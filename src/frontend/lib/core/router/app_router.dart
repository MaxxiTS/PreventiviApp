import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../features/home/presentation/home_screen.dart';
import '../../features/preciosarios/presentation/preciosarios_screen.dart';
import '../../features/presupuestos/presentation/presupuesto_screen.dart';
import '../../features/proyectos/presentation/proyecto_detalle_screen.dart';
import '../../features/proyectos/presentation/proyectos_screen.dart';
import 'app_shell.dart';

/// Provider del [GoRouter] de la aplicación.
///
/// Rutas:
///   '/'                    -> Inicio
///   '/proyectos'           -> Lista de proyectos
///   '/proyectos/:id'       -> Detalle de proyecto (+ presupuestos)
///   '/preciosarios'        -> Catálogo de precios (capítulos → partidas)
///   '/presupuestos/:id'    -> Detalle de presupuesto (resumen + árbol)
///
/// Todas las rutas viven dentro de un [ShellRoute] con el [AppShell]
/// (sidebar persistente + contenido).
final routerProvider = Provider<GoRouter>((ref) {
  return GoRouter(
    initialLocation: '/',
    routes: [
      ShellRoute(
        builder: (context, state, child) => AppShell(child: child),
        routes: [
          GoRoute(
            path: '/',
            builder: (context, state) => const HomeScreen(),
          ),
          GoRoute(
            path: '/proyectos',
            builder: (context, state) => const ProyectosScreen(),
            routes: [
              GoRoute(
                path: ':id',
                builder: (context, state) => ProyectoDetalleScreen(
                  proyectoId: state.pathParameters['id']!,
                ),
              ),
            ],
          ),
          GoRoute(
            path: '/preciosarios',
            builder: (context, state) => const PreciosariosScreen(),
          ),
          GoRoute(
            path: '/presupuestos/:id',
            builder: (context, state) => PresupuestoScreen(
              presupuestoId: state.pathParameters['id']!,
            ),
          ),
        ],
      ),
    ],
    errorBuilder: (context, state) => Scaffold(
      body: Center(
        child: Text('Ruta no encontrada: ${state.uri}'),
      ),
    ),
  );
});
