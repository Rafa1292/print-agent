# Troubleshooting - PrintAgent

## Problemas comunes y soluciones

---

### El servicio no inicia

**Síntomas:**
- El icono de PrintAgent no aparece en la bandeja del sistema
- La web muestra "No conectado"

**Soluciones:**

1. **Verificar que el servicio esté instalado:**
   ```powershell
   Get-Service -Name "PrintAgent.Service"
   ```

2. **Iniciar el servicio manualmente:**
   ```powershell
   Start-Service -Name "PrintAgent.Service"
   ```

3. **Verificar logs del servicio:**
   ```powershell
   Get-EventLog -LogName Application -Source "PrintAgent.Service" -Newest 10
   ```

4. **Reinstalar el servicio:**
   - Desinstalar desde "Agregar o quitar programas"
   - Volver a ejecutar el instalador como administrador

---

### La impresora no imprime

**Síntomas:**
- El servicio está conectado pero no sale el ticket
- Error "PRINT_ERROR" en la respuesta

**Soluciones:**

1. **Verificar que la impresora esté encendida y conectada**

2. **Verificar el nombre de la impresora en Windows:**
   - Ir a Configuración → Dispositivos → Impresoras
   - El nombre debe coincidir exactamente con el configurado en PrintAgent

3. **Probar impresión desde Windows:**
   - Click derecho en la impresora → Propiedades de impresora
   - Imprimir página de prueba

4. **Verificar el puerto de la impresora:**
   - Propiedades de la impresora → Puertos
   - Asegurarse de que el puerto USB/COM esté seleccionado

5. **Reiniciar el servicio de cola de impresión:**
   ```powershell
   Restart-Service -Name "Spooler"
   ```

---

### Error "Impresora no configurada"

**Síntomas:**
- Error `PRINTER_NOT_FOUND` al imprimir

**Solución:**

1. Abrir la UI de PrintAgent (desde menú inicio o bandeja)
2. Agregar la impresora con el nombre correcto
3. Asegurarse de que el "Nombre" coincida con lo que envía la web (ej: "factura", "cocina")

---

### El firewall bloquea la conexión

**Síntomas:**
- Desde la web no se puede conectar al agente
- `localhost:5123` no responde

**Solución:**

1. **Agregar regla de firewall manualmente:**
   ```powershell
   netsh advfirewall firewall add rule name="PrintAgent" dir=in action=allow protocol=TCP localport=5123
   ```

2. **O desde la UI de Windows Firewall:**
   - Panel de control → Firewall de Windows → Configuración avanzada
   - Reglas de entrada → Nueva regla
   - Puerto → TCP → 5123 → Permitir

---

### El ticket sale cortado o con caracteres extraños

**Síntomas:**
- El texto se corta antes de terminar la línea
- Aparecen símbolos raros en lugar de letras

**Soluciones:**

1. **Ajustar el ancho de papel:**
   - Abrir UI de PrintAgent
   - Editar la impresora
   - Cambiar "Ancho de papel" (típicamente 42 para 58mm, 48 para 80mm)

2. **Verificar codificación:**
   - La impresora debe soportar ESC/POS
   - Algunas impresoras genéricas no soportan todos los comandos

---

### El servicio consume mucha memoria

**Síntomas:**
- PrintAgent.Service.exe usa más de 200MB de RAM

**Solución:**

Esto es normal para aplicaciones .NET self-contained. El ejecutable incluye todo el runtime de .NET para no requerir instalación adicional.

Si es un problema, se puede compilar como framework-dependent (requiere .NET 8 instalado):
```powershell
dotnet publish -c Release --self-contained false
```

---

### No puedo desinstalar el servicio

**Síntomas:**
- El desinstalador falla
- El servicio queda "marcado para eliminación"

**Solución:**

1. **Detener el servicio primero:**
   ```powershell
   Stop-Service -Name "PrintAgent.Service" -Force
   ```

2. **Eliminar manualmente:**
   ```powershell
   sc.exe delete "PrintAgent.Service"
   ```

3. **Si dice "marcado para eliminación", reiniciar Windows**

---

### La UI no conecta con el servicio

**Síntomas:**
- La UI muestra "Servicio no disponible"
- Pero el servicio está corriendo

**Solución:**

1. Verificar que el servicio esté en el puerto correcto:
   ```powershell
   netstat -an | findstr "5123"
   ```

2. Si no aparece, revisar `appsettings.json`:
   ```json
   {
     "Urls": "http://localhost:5123"
   }
   ```

---

## Logs y diagnóstico

### Ver logs del servicio

Los logs se escriben en el Event Log de Windows:

```powershell
# Ver últimos 20 eventos
Get-EventLog -LogName Application -Source "PrintAgent*" -Newest 20

# Filtrar solo errores
Get-EventLog -LogName Application -Source "PrintAgent*" -EntryType Error -Newest 10
```

### Probar el servicio manualmente

```powershell
# Health check
Invoke-RestMethod http://localhost:5123/health

# Ver impresoras configuradas
Invoke-RestMethod http://localhost:5123/printers

# Ver impresoras del sistema
Invoke-RestMethod http://localhost:5123/printers/system

# Prueba de impresión
Invoke-RestMethod -Method Post -Uri http://localhost:5123/print/test -ContentType "application/json" -Body '{"printerName":"factura"}'
```

### Ejecutar el servicio en modo consola (debug)

```powershell
cd "C:\Program Files\PrintAgent\service"
.\PrintAgent.Service.exe
```

Esto ejecuta el servicio en primer plano y muestra los logs en la consola.

---

## Contacto y soporte

Si el problema persiste:

1. Revisar los issues en GitHub: https://github.com/Rafa1292/print-agent/issues
2. Crear un nuevo issue con:
   - Descripción del problema
   - Pasos para reproducir
   - Versión de Windows
   - Modelo de impresora
   - Logs relevantes
