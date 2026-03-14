// === LOGGER SYSTEM ===
// Zbiera klikniecia, błędy, navigacje i problemy

const Logger = {
    logs: [],
    maxLogs: 500,

    // Główna funkcja do logowania
    log(type, message, details = {}) {
        const logEntry = {
            timestamp: new Date().toISOString(),
            type: type, // 'click', 'error', 'navigation', 'warning', 'info'
            message: message,
            details: details,
            url: window.location.href
        };

        this.logs.push(logEntry);

        // Ogranicz ilość logów w pamięci
        if (this.logs.length > this.maxLogs) {
            this.logs.shift();
        }

        // Wyświetl w konsoli
        console.log(`[${type.toUpperCase()}] ${message}`, logEntry);

        // Wysłij na serwer (async, bez blokowania)
        this.sendToServer(logEntry);
    },

    // Loguj klikniecia
    logClick(event) {
        const target = event.target;
        const isLink = target.tagName === 'A' || target.closest('a');
        const isButton = target.tagName === 'BUTTON' || target.closest('button');
        const text = target.textContent?.trim().substring(0, 100) || target.id || target.className;

        this.log('click', `Clicked: ${target.tagName}`, {
            tag: target.tagName,
            text: text,
            href: isLink ? target.href : null,
            classes: target.className,
            id: target.id,
            x: event.clientX,
            y: event.clientY
        });
    },

    // Loguj błędy
    logError(message, source, lineno, colno, error) {
        this.log('error', message, {
            source: source,
            line: lineno,
            column: colno,
            stack: error?.stack || 'No stack trace'
        });
    },

    // Loguj navigację
    logNavigation(url) {
        this.log('navigation', `Navigated to: ${url}`, {
            url: url,
            referrer: document.referrer
        });
    },

    // Loguj warunki/ostrzeżenia
    logWarning(message, details = {}) {
        this.log('warning', message, details);
    },

    // Loguj info
    logInfo(message, details = {}) {
        this.log('info', message, details);
    },

    // Pobierz wszystkie logi
    getLogs(type = null) {
        if (type) {
            return this.logs.filter(log => log.type === type);
        }
        return this.logs;
    },

    // Eksportuj logi jako JSON
    exportLogsAsJSON() {
        return JSON.stringify(this.logs, null, 2);
    },

    // Eksportuj logi jako CSV
    exportLogsAsCSV() {
        if (this.logs.length === 0) return 'No logs';

        const headers = ['Timestamp', 'Type', 'Message', 'URL', 'Details'];
        const rows = this.logs.map(log => [
            log.timestamp,
            log.type,
            log.message,
            log.url,
            JSON.stringify(log.details)
        ]);

        const csv = [
            headers.join(','),
            ...rows.map(row => row.map(cell => `"${cell?.toString().replace(/"/g, '""')}"`).join(','))
        ].join('\n');

        return csv;
    },

    // Wyślij logi na serwer
    sendToServer(logEntry) {
        try {
            fetch('/_api/logs', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(logEntry)
            }).catch(err => console.warn('Failed to send log to server:', err));
        } catch (e) {
            console.warn('Logger: Could not send to server', e);
        }
    },

    // Wyczyść logi
    clearLogs() {
        this.logs = [];
        this.log('info', 'Logs cleared');
    },

    // Wyświetl statystykę
    printStats() {
        const stats = {};
        this.logs.forEach(log => {
            stats[log.type] = (stats[log.type] || 0) + 1;
        });
        console.table(stats);
        return stats;
    }
};

// === SETUP ===

// Loguj wszystkie klikniecia
document.addEventListener('click', (e) => Logger.logClick(e), true);

// Loguj wszystkie błędy
window.addEventListener('error', (e) => {
    Logger.logError(e.message, e.filename, e.lineno, e.colno, e.error);
});

// Loguj unhandled promise rejections
window.addEventListener('unhandledrejection', (e) => {
    Logger.log('error', 'Unhandled Promise Rejection', {
        reason: e.reason?.toString(),
        stack: e.reason?.stack
    });
});

// Loguj page visibility changes
document.addEventListener('visibilitychange', () => {
    Logger.logInfo(`Page ${document.hidden ? 'hidden' : 'visible'}`);
});

// Loguj page unload
window.addEventListener('beforeunload', () => {
    Logger.logInfo('Page unloading');
});

// Loguj perf metrics
window.addEventListener('load', () => {
    setTimeout(() => {
        const perf = window.performance.getEntriesByType('navigation')[0];
        if (perf) {
            Logger.logInfo('Page Load Complete', {
                domInteractive: perf.domInteractive,
                domComplete: perf.domComplete,
                loadEventEnd: perf.loadEventEnd,
                duration: perf.duration
            });
        }
    }, 1000);
});

// Udostępnij Logger w globalnym scope
window.Logger = Logger;

// Wyświetl welcome message
console.log('%c=== LOGGER ACTIVE ===', 'color: green; font-weight: bold;');
console.log('Use: Logger.log(type, message, details)');
console.log('Usage examples:');
console.log('  Logger.getLogs()         - Get all logs');
console.log('  Logger.getLogs("error")  - Get only errors');
console.log('  Logger.printStats()      - Show statistics');
console.log('  Logger.exportLogsAsJSON() - Export as JSON');
console.log('  Logger.exportLogsAsCSV()  - Export as CSV');
