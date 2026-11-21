/**
 * PetStore Real-Time Socket Manager (SignalR / Socket.io Architecture)
 * รองรับการสื่อสารแบบ Real-Time ระหว่างบทบาทต่างๆ และมี Socket.io Compatible Interface
 */
(function () {
    'use strict';

    // 1. ตรวจสอบว่ามี SignalR Library หรือไม่
    if (typeof signalR === 'undefined') {
        console.warn('[Socket] SignalR client library is not loaded. Real-time events disabled.');
        return;
    }

    // 2. สร้างการเชื่อมต่อ Hub Connection
    const connection = new signalR.HubConnectionBuilder()
        .withUrl('/petShopHub')
        .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
        .configureLogging(signalR.LogLevel.Warning)
        .build();

    // 3. Socket.io Compatibility Layer (รองรับ window.socket และ window.io)
    const socketIoCompat = {
        connection: connection,
        connected: false,
        id: 'petstore-client-' + Math.random().toString(36).substring(2, 9),
        on: function (eventName, callback) {
            connection.on(eventName, callback);
            return this;
        },
        off: function (eventName, callback) {
            if (callback) {
                connection.off(eventName, callback);
            } else {
                connection.off(eventName);
            }
            return this;
        },
        emit: function (eventName, ...args) {
            return connection.invoke(eventName, ...args);
        }
    };

    window.socket = socketIoCompat;
    window.petStoreSocket = socketIoCompat;
    window.io = function () { return socketIoCompat; };

    // 4. ระบบเสียงแจ้งเตือนแบบ Web Audio API (ไม่ต้องพึ่งไฟล์เสียงภายนอก)
    function playNotificationSound(type = 'chime') {
        try {
            const AudioCtx = window.AudioContext || window.webkitAudioContext;
            if (!AudioCtx) return;
            const ctx = new AudioCtx();
            const now = ctx.currentTime;

            if (type === 'alert') {
                // Warning chime: Double beep (A5 -> F5)
                const osc = ctx.createOscillator();
                const gain = ctx.createGain();
                osc.type = 'triangle';
                osc.frequency.setValueAtTime(880, now);
                osc.frequency.setValueAtTime(698, now + 0.12);
                gain.gain.setValueAtTime(0.15, now);
                gain.gain.exponentialRampToValueAtTime(0.001, now + 0.35);
                osc.connect(gain);
                gain.connect(ctx.destination);
                osc.start(now);
                osc.stop(now + 0.35);
            } else {
                // Success chime: Ascending melodic chord (C5 -> E5 -> G5)
                [523.25, 659.25, 783.99].forEach((freq, idx) => {
                    const osc = ctx.createOscillator();
                    const gain = ctx.createGain();
                    osc.type = 'sine';
                    osc.frequency.value = freq;
                    gain.gain.setValueAtTime(0.08, now + idx * 0.08);
                    gain.gain.exponentialRampToValueAtTime(0.001, now + idx * 0.08 + 0.3);
                    osc.connect(gain);
                    gain.connect(ctx.destination);
                    osc.start(now + idx * 0.08);
                    osc.stop(now + idx * 0.08 + 0.3);
                });
            }
        } catch (e) {
            // Audio context not allowed before user interaction, safely ignore
        }
    }

    // 5. ระบบ Real-Time Toast Notification
    function showRealTimeToast(title, message, iconClass = 'bi-bell-fill', variant = 'gold') {
        let container = document.getElementById('socket-toast-container');
        if (!container) {
            container = document.createElement('div');
            container.id = 'socket-toast-container';
            container.style.cssText = 'position: fixed; bottom: 24px; right: 24px; z-index: 99999; display: flex; flex-direction: column; gap: 10px; max-width: 380px; pointer-events: none;';
            document.body.appendChild(container);
        }

        const toast = document.createElement('div');
        toast.style.cssText = `
            pointer-events: auto;
            background: rgba(30, 24, 20, 0.95);
            backdrop-filter: blur(10px);
            color: #F8F5EE;
            border: 1px solid rgba(223, 195, 168, 0.35);
            box-shadow: 0 10px 25px rgba(0,0,0,0.35);
            border-radius: 12px;
            padding: 12px 16px;
            font-family: 'Prompt', sans-serif;
            font-size: 0.85rem;
            display: flex;
            align-items: flex-start;
            gap: 12px;
            opacity: 0;
            transform: translateY(16px);
            transition: all 0.3s cubic-bezier(0.16, 1, 0.3, 1);
        `;

        const badgeColor = variant === 'danger' ? '#ef4444' : (variant === 'warning' ? '#f59e0b' : '#dfc3a8');

        toast.innerHTML = `
            <div style="background: rgba(223, 195, 168, 0.15); color: ${badgeColor}; width: 34px; height: 34px; border-radius: 8px; display: flex; align-items: center; justify-content: center; flex-shrink: 0; font-size: 1.1rem;">
                <i class="bi ${iconClass}"></i>
            </div>
            <div style="flex: 1; min-width: 0;">
                <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 2px;">
                    <span style="font-weight: 600; font-size: 0.82rem; color: #dfc3a8;">${title}</span>
                    <span style="font-size: 0.68rem; color: #a89f91;">ตอนนี้</span>
                </div>
                <div style="font-size: 0.8rem; color: #ece4d8; line-height: 1.4;">${message}</div>
            </div>
            <button type="button" style="background: none; border: none; color: #a89f91; cursor: pointer; padding: 2px; line-height: 1;" onclick="this.parentElement.remove()">
                <i class="bi bi-x-lg" style="font-size: 0.75rem;"></i>
            </button>
        `;

        container.appendChild(toast);

        // Slide in animation
        requestAnimationFrame(() => {
            toast.style.opacity = '1';
            toast.style.transform = 'translateY(0)';
        });

        // Auto remove after 6 seconds
        setTimeout(() => {
            toast.style.opacity = '0';
            toast.style.transform = 'translateY(10px)';
            setTimeout(() => toast.remove(), 350);
        }, 6000);
    }

    // 6. ลงทะเบียน Event Listeners สำหรับ Real-Time Events
    // Event A: แจ้งเตือนคำสั่งซื้อใหม่ (Order Staff, Accounting, Owner)
    connection.on('ReceiveNewOrderAlert', function (orderId, customerName, grandTotal) {
        console.log(`[Socket] New Order #${orderId} from ${customerName}: ฿${grandTotal}`);
        playNotificationSound('chime');
        showRealTimeToast(
            'คำสั่งซื้อใหม่ในระบบ (Real-Time)',
            `ออเดอร์ <strong>#${orderId}</strong> จากคุณ <strong>${customerName}</strong> ยอดชำระ ฿${Number(grandTotal).toLocaleString('th-TH')}`,
            'bi-bag-check-fill',
            'gold'
        );

        // อัปเดตตัวเลขคำสั่งซื้อบน Badge หรือรีเฟรชตารางถ้าอยู่ในหน้า Order
        const orderBadge = document.querySelector('.badge-pending-orders');
        if (orderBadge) {
            const current = parseInt(orderBadge.textContent) || 0;
            orderBadge.textContent = current + 1;
        }
    });

    // Event B: อัปเดตสต็อกคงเหลือแบบ Real-Time (Storefront & Inventory)
    connection.on('ReceiveStockUpdate', function (productId, newStock) {
        console.log(`[Socket] Stock update for Product #${productId}: ${newStock}`);
        
        // อัปเดตสต็อกในหน้าร้านค้า หรือหน้าคลังสินค้า
        const stockElements = document.querySelectorAll(`[data-product-id="${productId}"] .stock-display, #stock-${productId}`);
        stockElements.forEach(el => {
            el.textContent = `${newStock} ชิ้น`;
            el.style.transition = 'color 0.3s';
            el.style.color = '#10b981';
            setTimeout(() => { el.style.color = ''; }, 2000);
        });

        // อัปเดตในตารางคลังสินค้าหลังบ้านถ้าเปิดอยู่
        const tableRow = document.querySelector(`tr[data-product-row="${productId}"] .stock-quantity`);
        if (tableRow) {
            tableRow.textContent = `${newStock} ชิ้น`;
        }
    });

    // Event C: แจ้งเตือนสินค้าใกล้หมด Low Stock Alert (Req 18)
    connection.on('ReceiveLowStockAlert', function (productName, currentStock) {
        console.warn(`[Socket] Low stock warning: ${productName} (Stock: ${currentStock})`);
        playNotificationSound('alert');
        showRealTimeToast(
            'แจ้งเตือนคลังสินค้า (Low Stock)',
            `สินค้า <strong>${productName}</strong> เหลือเพียง <strong>${currentStock} ชิ้น</strong> ต่ำกว่าเกณฑ์ที่กำหนด!`,
            'bi-exclamation-triangle-fill',
            'warning'
        );
    });

    // Event D: แจ้งเตือนสถานะพัสดุจัดส่งแล้ว (Shipping Staff -> Customer)
    connection.on('ReceiveOrderShippedAlert', function (orderId, trackingNumber, courier) {
        console.log(`[Socket] Order Shipped #${orderId}: ${trackingNumber} (${courier})`);
        playNotificationSound('chime');
        showRealTimeToast(
            'พัสดุของคุณถูกจัดส่งแล้ว!',
            `คำสั่งซื้อ <strong>#${orderId}</strong> ขนส่งโดย <strong>${courier}</strong> เลข Tracking: <strong>${trackingNumber}</strong>`,
            'bi-truck',
            'gold'
        );
    });

    // Event E: แจ้งเตือนจำนวนคนใส่ตะกร้า
    connection.on('ReceiveCartCountUpdate', function (productId, cartCount) {
        // Disabled per user request (ลบป้าย mock ในตะกร้าออกทั้งหมด)
    });

    // 7. เริ่มการเชื่อมต่อ WebSocket Hub
    connection.start()
        .then(() => {
            socketIoCompat.connected = true;
            console.log('%c[Socket.io / SignalR] Connected successfully to /petShopHub', 'color: #10b981; font-weight: bold;');
        })
        .catch(err => {
            console.error('[Socket] Connection failed:', err.toString());
        });

    connection.onreconnected(() => {
        socketIoCompat.connected = true;
        console.log('[Socket] Reconnected to /petShopHub');
    });

    connection.onclose(() => {
        socketIoCompat.connected = false;
        console.warn('[Socket] Connection closed');
    });

})();
