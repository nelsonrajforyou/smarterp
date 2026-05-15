/**
 * SCHOOL ERP - THEME MANAGER
 * Persists dark/light mode, colors, fonts across all Blazor navigations.
 * Uses MutationObserver to fight Blazor's DOM morphing on enhanced navigation.
 */
window.themeManager = {

    // ── Apply Helpers ──────────────────────────────────────────────────────
    hexToRgb: function(hex) {
        var result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
        return result ? {
            r: parseInt(result[1], 16),
            g: parseInt(result[2], 16),
            b: parseInt(result[3], 16)
        } : null;
    },

    applyTheme: function (color) {
        document.documentElement.style.setProperty('--primary-500', color);
        const rgb = this.hexToRgb(color);
        if (rgb) {
            document.documentElement.style.setProperty('--primary-rgb', `${rgb.r}, ${rgb.g}, ${rgb.b}`);
        }
        localStorage.setItem('user-theme-color', color);
    },

    applyFont: function (fontFamily) {
        document.documentElement.style.setProperty('--font-family', fontFamily);
        document.documentElement.style.fontFamily = fontFamily;
        localStorage.setItem('user-font-family', fontFamily);
    },

    applyFontSize: function (size) {
        document.documentElement.style.setProperty('--base-font-size', size);
        document.documentElement.style.fontSize = size;
        localStorage.setItem('user-font-size', size);
    },

    applyDarkMode: function (isDark) {
        document.documentElement.setAttribute('data-theme', isDark ? 'dark' : 'light');
        localStorage.setItem('user-dark-mode', isDark ? 'true' : 'false');
    },

    toggleDarkMode: function () {
        const current = document.documentElement.getAttribute('data-theme');
        const next = (current === 'dark') ? 'light' : 'dark';
        document.documentElement.setAttribute('data-theme', next);
        localStorage.setItem('user-dark-mode', next === 'dark' ? 'true' : 'false');
        return next === 'dark';
    },

    // ── Read Settings (called from Blazor on load) ─────────────────────────

    getSettings: function () {
        return {
            color:    localStorage.getItem('user-theme-color')   || '#6366f1',
            font:     localStorage.getItem('user-font-family')   || "'Inter', sans-serif",
            fontSize: localStorage.getItem('user-font-size')     || '16px',
            darkMode: localStorage.getItem('user-dark-mode')     === 'true'
        };
    },

    // ── Load All Settings ──────────────────────────────────────────────────

    loadSettings: function () {
        const color    = localStorage.getItem('user-theme-color');
        const font     = localStorage.getItem('user-font-family');
        const size     = localStorage.getItem('user-font-size');
        const darkMode = localStorage.getItem('user-dark-mode');

        if (color) {
            document.documentElement.style.setProperty('--primary-500', color);
            const rgb = this.hexToRgb(color);
            if (rgb) {
                document.documentElement.style.setProperty('--primary-rgb', `${rgb.r}, ${rgb.g}, ${rgb.b}`);
            }
        }
        if (font)  document.documentElement.style.setProperty('--font-family', font);
        if (size)  document.documentElement.style.setProperty('--base-font-size', size);

        const isDark = darkMode !== null ? darkMode === 'true' : false;
        document.documentElement.setAttribute('data-theme', isDark ? 'dark' : 'light');
    }
};

// ── Apply immediately on page load ────────────────────────────────────────
themeManager.loadSettings();

// ── MutationObserver: defend against Blazor DOM morphing ─────────────────
(function () {
    var observer = new MutationObserver(function (mutations) {
        mutations.forEach(function (mutation) {
            if (mutation.attributeName === 'data-theme' || mutation.attributeName === 'style') {
                // Throttle re-apply to avoid loops if needed, but usually fine
                var currentTheme = document.documentElement.getAttribute('data-theme');
                var savedTheme   = localStorage.getItem('user-dark-mode') === 'true' ? 'dark' : 'light';
                
                var color = localStorage.getItem('user-theme-color');
                var hasPrimary = document.documentElement.style.getPropertyValue('--primary-500');

                if (currentTheme !== savedTheme || (color && !hasPrimary)) {
                    themeManager.loadSettings();
                }
            }
        });
    });

    observer.observe(document.documentElement, {
        attributes: true,
        attributeFilter: ['data-theme', 'style']
    });
})();

// ── Re-apply after Blazor enhanced navigation ─────────────────────────────
document.addEventListener('blazor:navigated', function () {
    // Small delay ensures Blazor finishes its DOM morphing before we restore
    setTimeout(function () { themeManager.loadSettings(); }, 0);
});

document.addEventListener('enhancedload', function () {
    setTimeout(function () { themeManager.loadSettings(); }, 0);
});
