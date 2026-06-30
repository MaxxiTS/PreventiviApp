import 'dart:typed_data';

import 'package:file_picker/file_picker.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';

import '../../../shared/utils/errores.dart';

/// Guarda los [bytes] de un PDF mediante un diálogo del sistema "Guardar como".
///
/// Usa `file_picker` (`saveFile`), que funciona en escritorio, móvil y web.
/// En las plataformas que lo soportan se escriben directamente los bytes; en
/// las demás, el usuario elige la ruta y la propia librería gestiona el guardado.
Future<void> guardarPdf(
  BuildContext context,
  List<int> bytes,
  String nombreSugerido,
) async {
  if (bytes.isEmpty) {
    mostrarErrorSnack(context, 'El PDF recibido está vacío.');
    return;
  }
  try {
    final ruta = await FilePicker.platform.saveFile(
      dialogTitle: 'Guardar presupuesto en PDF',
      fileName: nombreSugerido,
      type: FileType.custom,
      allowedExtensions: const ['pdf'],
      bytes: Uint8List.fromList(bytes),
    );
    if (!context.mounted) return;
    if (ruta == null) {
      // El usuario canceló el diálogo.
      return;
    }
    // En web, saveFile ya dispara la descarga y `ruta` es solo informativa.
    mostrarSnack(
      context,
      kIsWeb ? 'Descarga iniciada.' : 'PDF guardado en: $ruta',
    );
  } catch (e) {
    if (context.mounted) mostrarErrorSnack(context, e);
  }
}
