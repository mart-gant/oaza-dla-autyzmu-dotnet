// Dark Mode Toggle
function initDarkMode() {
    const darkModeToggle = document.getElementById('darkModeToggle');
    const html = document.documentElement;
    
    // Check saved preference or system preference
    const savedTheme = localStorage.getItem('theme');
    const systemPrefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
    
    if (savedTheme === 'dark' || (!savedTheme && systemPrefersDark)) {
        html.classList.add('dark');
        if (darkModeToggle) darkModeToggle.checked = true;
    }
    
    // Toggle dark mode
    if (darkModeToggle) {
        darkModeToggle.addEventListener('change', () => {
            if (darkModeToggle.checked) {
                html.classList.add('dark');
                localStorage.setItem('theme', 'dark');
            } else {
                html.classList.remove('dark');
                localStorage.setItem('theme', 'light');
            }
        });
    }
}

// Accessibility Preferences
function initAccessibilityPreferences() {
    const html = document.documentElement;

    const toggleMap = [
        { key: 'highContrast', attr: 'data-contrast', value: 'high', ids: ['highContrastToggle', 'highContrastToggleMobile'] },
        { key: 'largeText', attr: 'data-font-size', value: 'large', ids: ['fontSizeToggle', 'fontSizeToggleMobile'] },
        { key: 'reducedMotion', attr: 'data-reduce-motion', value: 'true', ids: ['reducedMotionToggle', 'reducedMotionToggleMobile'] },
        { key: 'calmMode', attr: 'data-calm', value: 'true', ids: ['calmModeToggle', 'calmModeToggleMobile'] }
    ];

    toggleMap.forEach(setting => {
        const saved = localStorage.getItem(setting.key);
        if (saved === 'true') {
            html.setAttribute(setting.attr, setting.value);
        } else {
            html.removeAttribute(setting.attr);
        }

        setting.ids.forEach(id => {
            const toggle = document.getElementById(id);
            if (!toggle) return;
            toggle.checked = saved === 'true';
            toggle.addEventListener('change', () => {
                if (toggle.checked) {
                    localStorage.setItem(setting.key, 'true');
                    html.setAttribute(setting.attr, setting.value);
                } else {
                    localStorage.setItem(setting.key, 'false');
                    html.removeAttribute(setting.attr);
                }

                // Sync paired mobile/desktop toggles
                setting.ids.forEach(otherId => {
                    const other = document.getElementById(otherId);
                    if (other && other !== toggle) {
                        other.checked = toggle.checked;
                    }
                });
            });
        });
    });

    const prefersReducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    if (prefersReducedMotion && localStorage.getItem('reducedMotion') !== 'false') {
        html.setAttribute('data-reduce-motion', 'true');
        localStorage.setItem('reducedMotion', 'true');
        ['reducedMotionToggle', 'reducedMotionToggleMobile'].forEach(id => {
            const toggle = document.getElementById(id);
            if (toggle) toggle.checked = true;
        });
    }
}

// Mobile Menu Toggle
function initMobileMenu() {
    const mobileMenuButton = document.getElementById('mobileMenuButton');
    const mobileMenu = document.getElementById('mobileMenu');
    
    if (mobileMenuButton && mobileMenu) {
        mobileMenuButton.addEventListener('click', () => {
            mobileMenu.classList.toggle('hidden');
            const isExpanded = !mobileMenu.classList.contains('hidden');
            mobileMenuButton.setAttribute('aria-expanded', isExpanded.toString());
        });
    }
}

// Loading State for Forms
function addLoadingStates() {
    const forms = document.querySelectorAll('form[data-loading="true"]');
    
    forms.forEach(form => {
        form.addEventListener('submit', (e) => {
            const submitButton = form.querySelector('button[type="submit"]');
            if (submitButton) {
                submitButton.disabled = true;
                submitButton.innerHTML = `
                    <svg class="animate-spin h-5 w-5 inline mr-2" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                        <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                        <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                    </svg>
                    Ładowanie...
                `;
            }
        });
    });
}

// Lazy Load Images
function initLazyLoading() {
    const images = document.querySelectorAll('img[loading="lazy"]');
    
    if ('IntersectionObserver' in window) {
        const imageObserver = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const img = entry.target;
                    img.src = img.dataset.src || img.src;
                    img.classList.add('loaded');
                    imageObserver.unobserve(img);
                }
            });
        });
        
        images.forEach(img => imageObserver.observe(img));
    }
}

// Toast Notifications
function showToast(message, type = 'info') {
    const toast = document.createElement('div');
    toast.setAttribute('role', 'status');
    toast.setAttribute('aria-live', 'polite');
    toast.className = `fixed bottom-4 right-4 z-50 px-6 py-3 rounded-lg shadow-lg transform transition-all duration-300 ${
        type === 'success' ? 'bg-green-600 text-white' :
        type === 'error' ? 'bg-red-600 text-white' :
        type === 'warning' ? 'bg-yellow-600 text-white' :
        'bg-blue-600 text-white'
    }`;
    toast.textContent = message;
    document.body.appendChild(toast);
    
    setTimeout(() => toast.classList.add('translate-x-0'), 100);
    setTimeout(() => {
        toast.classList.add('opacity-0', 'translate-x-full');
        setTimeout(() => toast.remove(), 300);
    }, 3000);
}

// Initialize on DOM load
document.addEventListener('DOMContentLoaded', () => {
    initDarkMode();
    initAccessibilityPreferences();
    initMobileMenu();
    addLoadingStates();
    initLazyLoading();
    
    // Show success/error messages from TempData
    const successMsg = document.querySelector('[data-success-message]');
    const errorMsg = document.querySelector('[data-error-message]');
    
    if (successMsg) showToast(successMsg.dataset.successMessage, 'success');
    if (errorMsg) showToast(errorMsg.dataset.errorMessage, 'error');
});
