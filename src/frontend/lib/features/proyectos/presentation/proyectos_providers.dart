import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../data/proyectos_repository.dart';
import '../domain/proyecto.dart';

/// Lista de proyectos. Se refresca con `ref.invalidate(proyectosProvider)`.
final proyectosProvider = FutureProvider<List<Proyecto>>((ref) {
  return ref.watch(proyectosRepositoryProvider).listar();
});

/// Detalle de un proyecto por id (family).
final proyectoProvider =
    FutureProvider.family<Proyecto, String>((ref, id) {
  return ref.watch(proyectosRepositoryProvider).obtener(id);
});
