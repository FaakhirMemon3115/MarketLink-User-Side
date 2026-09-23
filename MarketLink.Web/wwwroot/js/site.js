/* MarketLink – eGreen Basket | Main JavaScript */
'use strict';

/* ── Dark Mode ──────────────────────────────────────────────── */
const DarkMode = {
    init() {
        const saved = localStorage.getItem('ml-theme') || 'light';
        document.documentElement.setAttribute('data-theme', saved);
        document.querySelectorAll('.dark-mode-toggle').forEach(btn => {
            btn.setAttribute('aria-checked', saved === 'dark');
            btn.addEventListener('click', () => this.toggle());
        });
    },
    toggle() {
        const current = document.documentElement.getAttribute('data-theme');
        const next = current === 'dark' ? 'light' : 'dark';
        document.documentElement.setAttribute('data-theme', next);
        localStorage.setItem('ml-theme', next);
        document.querySelectorAll('.dark-mode-toggle').forEach(btn => {
            btn.setAttribute('aria-checked', next === 'dark');
        });
    }
};

/* ── Toast Notifications ────────────────────────────────────── */
const Toast = {
    container: null,
    init() {
        this.container = document.querySelector('.toast-container-green');
        if (!this.container) {
            this.container = document.createElement('div');
            this.container.className = 'toast-container-green';
            document.body.appendChild(this.container);
        }
    },
    show(message, type = 'success', duration = 4000) {
        const icons = { success: '✅', error: '❌', warning: '⚠️', info: 'ℹ️' };
        const toast = document.createElement('div');
        toast.className = `toast-green toast-${type}`;
        toast.innerHTML = `
            <div style="display:flex;align-items:flex-start;gap:10px;">
                <span style="font-size:18px">${icons[type] || icons.info}</span>
                <div style="flex:1;">
                    <div style="font-size:14px;font-weight:600;color:var(--heading);margin-bottom:3px;">${type.charAt(0).toUpperCase() + type.slice(1)}</div>
                    <div style="font-size:13px;color:var(--body-text);">${message}</div>
                </div>
                <button onclick="this.closest('.toast-green').remove()" style="background:none;border:none;cursor:pointer;color:var(--muted);font-size:16px;">×</button>
            </div>`;
        this.container.appendChild(toast);
        setTimeout(() => toast.style.opacity = '0', duration - 300);
        setTimeout(() => toast.remove(), duration);
    }
};

/* ── Mobile & Desktop Hover Sidebar ─────────────────────────── */
const Sidebar = {
    init() {
        const sidebar = document.querySelector('.dashboard-sidebar');
        const overlay = document.querySelector('.dashboard-overlay');
        const toggleBtn = document.querySelector('#sidebar-toggle');
        if (!sidebar) return;

        // Restore collapsed state on desktop
        const isCollapsed = localStorage.getItem('ml-sidebar-collapsed') === 'true';
        if (isCollapsed && window.innerWidth >= 993) {
            document.body.classList.add('sidebar-hover-collapsed');
        }

        toggleBtn?.addEventListener('click', () => {
            if (window.innerWidth < 993) {
                // Mobile: toggle drawer offcanvas
                this.toggleMobile(sidebar, overlay);
            } else {
                // Desktop: toggle hover collapse
                document.body.classList.toggle('sidebar-hover-collapsed');
                const collapsed = document.body.classList.contains('sidebar-hover-collapsed');
                localStorage.setItem('ml-sidebar-collapsed', collapsed);
            }
        });

        overlay?.addEventListener('click', () => this.closeMobile(sidebar, overlay));

        // Auto close mobile drawer on clicking any nav item
        sidebar.querySelectorAll('.sidebar-nav-item').forEach(link => {
            link.addEventListener('click', () => {
                if (window.innerWidth < 993) {
                    this.closeMobile(sidebar, overlay);
                }
            });
        });
    },
    toggleMobile(sidebar, overlay) {
        sidebar.classList.toggle('open');
        overlay?.classList.toggle('active');
    },
    closeMobile(sidebar, overlay) {
        sidebar.classList.remove('open');
        overlay?.classList.remove('active');
    }
};

/* ── Cart AJAX ──────────────────────────────────────────────── */
const Cart = {
    async addItem(productId, quantityKg) {
        try {
            const response = await fetch('/Customer/Cart/AddItem', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': getAntiForgeryToken() },
                body: JSON.stringify({ productId, quantityKg })
            });
            const data = await response.json();
            if (data.success) {
                Toast.show(`Added ${quantityKg}kg to cart!`, 'success');
                this.updateBadge(data.cartCount);
            } else {
                Toast.show(data.message || 'Failed to add to cart', 'error');
            }
        } catch (e) {
            Toast.show('Network error. Please try again.', 'error');
        }
    },
    updateBadge(count) {
        document.querySelectorAll('.cart-badge').forEach(el => {
            el.textContent = count;
            el.style.display = count > 0 ? 'flex' : 'none';
        });
    }
};

/* ── Favorites AJAX ─────────────────────────────────────────── */
const Favorites = {
    async toggle(type, id, btn) {
        try {
            const response = await fetch('/Customer/Favorites/Toggle', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': getAntiForgeryToken() },
                body: JSON.stringify({ type, id })
            });
            const data = await response.json();
            if (data.success) {
                btn.classList.toggle('active', data.isFavorite);
                Toast.show(data.isFavorite ? 'Added to favorites!' : 'Removed from favorites', data.isFavorite ? 'success' : 'info');
            }
        } catch (e) {
            Toast.show('Please login to save favorites', 'warning');
        }
    }
};

/* ── Quantity Selector ──────────────────────────────────────── */
function initQtySelector() {
    document.querySelectorAll('.qty-option').forEach(opt => {
        opt.addEventListener('click', function () {
            const group = this.closest('.qty-selector');
            group?.querySelectorAll('.qty-option').forEach(o => o.classList.remove('selected'));
            this.classList.add('selected');
            const input = group?.nextElementSibling;
            if (input?.type === 'hidden') input.value = this.dataset.qty;
        });
    });
}

/* ── AI Search Assistant ────────────────────────────────────── */
const AIAssistant = {
    panel: null,
    input: null,
    messages: null,
    init() {
        this.panel = document.querySelector('.ai-chat-panel');
        this.input = document.querySelector('.ai-chat-input input');
        this.messages = document.querySelector('.ai-chat-messages');
        const toggle = document.querySelector('.ai-toggle-btn');
        const sendBtn = document.querySelector('.ai-chat-input button');

        toggle?.addEventListener('click', () => {
            this.panel?.classList.toggle('open');
            if (this.panel?.classList.contains('open') && this.messages?.children.length === 0) {
                this.addMessage('bot', '👋 Hi! I\'m your eGreen Basket assistant. Ask me about products, seasons, or what\'s fresh today!');
            }
        });

        this.input?.addEventListener('keypress', e => { if (e.key === 'Enter') this.sendMessage(); });
        sendBtn?.addEventListener('click', () => this.sendMessage());
    },
    addMessage(type, text) {
        const msg = document.createElement('div');
        msg.className = `ai-msg ${type}`;
        msg.textContent = text;
        this.messages?.appendChild(msg);
        if (this.messages) this.messages.scrollTop = this.messages.scrollHeight;
    },
    async sendMessage() {
        const text = this.input?.value.trim();
        if (!text) return;
        this.addMessage('user', text);
        if (this.input) this.input.value = '';

        // Smart search response
        const lower = text.toLowerCase();
        if (lower.includes('organic')) {
            this.addMessage('bot', '🌿 Looking for organic products! Let me search our organic section for you...');
            setTimeout(() => window.location.href = '/Products?isOrganic=true', 1500);
        } else if (lower.includes('tomato') || lower.includes('carrot') || lower.includes('potato')) {
            const word = lower.includes('tomato') ? 'tomato' : lower.includes('carrot') ? 'carrot' : 'potato';
            this.addMessage('bot', `🔍 Searching for ${word} products from local farmers...`);
            setTimeout(() => window.location.href = `/Products?q=${word}`, 1500);
        } else if (lower.includes('cheap') || lower.includes('affordable') || lower.includes('best price')) {
            this.addMessage('bot', '💰 Showing you the most affordable options sorted by price...');
            setTimeout(() => window.location.href = '/Products?sortBy=price_asc', 1500);
        } else if (lower.includes('seasonal') || lower.includes('season')) {
            this.addMessage('bot', '🍂 Here are our fresh seasonal products right now!');
            setTimeout(() => window.location.href = '/Products?seasonal=true', 1500);
        } else {
            this.addMessage('bot', `🔍 Searching for "${text}" across all our farmers and products...`);
            setTimeout(() => window.location.href = `/Products?q=${encodeURIComponent(text)}`, 1500);
        }
    }
};

/* ── Image Gallery ──────────────────────────────────────────── */
function initProductGallery() {
    const thumbs = document.querySelectorAll('.product-thumb');
    const mainImg = document.querySelector('#product-main-image');
    thumbs.forEach(thumb => {
        thumb.addEventListener('click', function () {
            if (mainImg) mainImg.src = this.dataset.full;
            thumbs.forEach(t => t.classList.remove('active'));
            this.classList.add('active');
        });
    });
}

/* ── Number Counter Animation ───────────────────────────────── */
function animateCounters() {
    document.querySelectorAll('[data-count]').forEach(el => {
        const target = parseInt(el.dataset.count);
        const duration = 1500;
        const step = target / (duration / 16);
        let current = 0;
        const timer = setInterval(() => {
            current = Math.min(current + step, target);
            el.textContent = Math.floor(current).toLocaleString();
            if (current >= target) clearInterval(timer);
        }, 16);
    });
}

/* ── Scroll Animations ──────────────────────────────────────── */
function initScrollAnimations() {
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('animate-fade-up');
                observer.unobserve(entry.target);
            }
        });
    }, { threshold: 0.1 });
    document.querySelectorAll('.animate-on-scroll').forEach(el => observer.observe(el));
}

/* ── Leaflet Map ────────────────────────────────────────────── */
function initMarketMap(mapId, locations) {
    if (!document.getElementById(mapId) || typeof L === 'undefined') return;
    const map = L.map(mapId).setView([39.5, -98.35], 4);
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '© <a href="https://www.openstreetmap.org/">OpenStreetMap</a> contributors'
    }).addTo(map);

    const greenIcon = L.icon({
        iconUrl: '/images/map-marker-green.png',
        iconSize: [32, 40],
        iconAnchor: [16, 40],
        popupAnchor: [0, -40]
    });

    locations?.forEach(loc => {
        L.marker([loc.lat, loc.lng], { icon: greenIcon })
            .addTo(map)
            .bindPopup(`<strong>${loc.name}</strong><br>${loc.address}`);
    });
    return map;
}

/* ── Charts (Chart.js) ──────────────────────────────────────── */
function initSalesChart(canvasId, labels, data) {
    const ctx = document.getElementById(canvasId);
    if (!ctx || typeof Chart === 'undefined') return;
    return new Chart(ctx, {
        type: 'line',
        data: {
            labels,
            datasets: [{
                label: 'Revenue',
                data,
                borderColor: '#5C6F2B',
                backgroundColor: 'rgba(92,111,43,.1)',
                borderWidth: 2,
                fill: true,
                tension: 0.4,
                pointBackgroundColor: '#5C6F2B',
                pointRadius: 4
            }]
        },
        options: {
            responsive: true,
            plugins: { legend: { display: false } },
            scales: {
                y: { beginAtZero: true, grid: { color: 'rgba(92,111,43,.08)' } },
                x: { grid: { display: false } }
            }
        }
    });
}

function initDonutChart(canvasId, labels, data, colors) {
    const ctx = document.getElementById(canvasId);
    if (!ctx || typeof Chart === 'undefined') return;
    return new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels,
            datasets: [{ data, backgroundColor: colors || ['#5C6F2B', '#DE802B', '#D8E983', '#9CAB84', '#D8C9A7'] }]
        },
        options: { responsive: true, plugins: { legend: { position: 'bottom' } }, cutout: '65%' }
    });
}

/* ── AJAX form helpers ──────────────────────────────────────── */
function getAntiForgeryToken() {
    return document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
}

async function postJson(url, data) {
    const res = await fetch(url, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': getAntiForgeryToken() },
        body: JSON.stringify(data)
    });
    return res.json();
}

/* ── Notifications Polling ──────────────────────────────────── */
const Notifications = {
    async refresh() {
        try {
            const data = await fetch('/Notifications/UnreadCount').then(r => r.json());
            document.querySelectorAll('.notif-badge').forEach(el => {
                el.textContent = data.count;
                el.style.display = data.count > 0 ? 'flex' : 'none';
            });
        } catch { /* silent */ }
    },
    startPolling(interval = 30000) {
        this.refresh();
        setInterval(() => this.refresh(), interval);
    }
};

/* ── Search Autocomplete ────────────────────────────────────── */
function initSearchAutocomplete(inputId) {
    const input = document.getElementById(inputId);
    if (!input) return;
    let timer;
    input.addEventListener('input', function () {
        clearTimeout(timer);
        const q = this.value.trim();
        if (q.length < 2) return;
        timer = setTimeout(async () => {
            try {
                const results = await fetch(`/Products/Autocomplete?q=${encodeURIComponent(q)}`).then(r => r.json());
                // Render dropdown
                let dropdown = document.getElementById('search-dropdown');
                if (!dropdown) {
                    dropdown = document.createElement('div');
                    dropdown.id = 'search-dropdown';
                    dropdown.style.cssText = 'position:absolute;top:100%;left:0;right:0;background:var(--card);border:1px solid var(--border);border-radius:var(--radius);box-shadow:var(--shadow-lg);z-index:1000;overflow:hidden;';
                    input.parentElement.style.position = 'relative';
                    input.parentElement.appendChild(dropdown);
                }
                dropdown.innerHTML = results.slice(0, 6).map(r =>
                    `<a href="/Products/${r.slug}" style="display:flex;align-items:center;gap:12px;padding:10px 16px;color:var(--body-text);font-size:14px;border-bottom:1px solid var(--border);">
                        <img src="${r.imageUrl || '/images/placeholder.png'}" style="width:36px;height:36px;border-radius:6px;object-fit:cover;">
                        <div><div style="font-weight:600;">${r.name}</div><div style="font-size:12px;color:var(--muted);">${r.categoryName} · $${r.pricePerKg}/kg</div></div>
                    </a>`
                ).join('');
                document.addEventListener('click', (e) => {
                    if (!dropdown.contains(e.target) && e.target !== input)
                        dropdown.innerHTML = '';
                }, { once: true });
            } catch { /* silent */ }
        }, 300);
    });
}

/* ── Init ───────────────────────────────────────────────────── */
document.addEventListener('DOMContentLoaded', () => {
    DarkMode.init();
    Toast.init();
    Sidebar.init();
    AIAssistant.init();
    initQtySelector();
    initProductGallery();
    initScrollAnimations();
    initSearchAutocomplete('nav-search');

    // Auto-dismiss alerts
    document.querySelectorAll('.alert.auto-dismiss').forEach(alert => {
        setTimeout(() => {
            alert.style.opacity = '0';
            setTimeout(() => alert.remove(), 300);
        }, 5000);
    });

    // Counter animation on visible
    const countersSection = document.querySelector('[data-counters]');
    if (countersSection) {
        const obs = new IntersectionObserver(entries => {
            if (entries[0].isIntersecting) { animateCounters(); obs.disconnect(); }
        });
        obs.observe(countersSection);
    }

    // Notification polling for authenticated users
    if (document.body.dataset.authenticated === 'true') {
        Notifications.startPolling();
    }
});

// Expose globally
window.ML = { Cart, Favorites, Toast, DarkMode, initSalesChart, initDonutChart, initMarketMap };
