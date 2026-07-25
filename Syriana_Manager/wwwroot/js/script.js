// Aktualisiere die CSS-Variable topTable mit der tatsächlichen Höhe von topTable, um die thead th mit position: sticky korrekt zu machen, und immer sichtbar bei Scrollen zu halten.
window.updateTopTableHeight = () => {
    const topTable = document.querySelector('.topTable');
    if (topTable) {
        const height = topTable.offsetHeight;
        // Wir speichern die tatsächliche Höhe in einer CSS-Variablen.
        document.documentElement.style.setProperty('--top-height', height + 'px');
    }
};

// langauge cocikes fixd
window.blazorCulture = {
    set: function (value) {
        const days = 365; // 1 Jahr
        const date = new Date();
        date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
        const expires = "expires=" + date.toUTCString();

        document.cookie =
            `.AspNetCore.Culture=c=${value}|uic=${value}; ${expires}; path=/; SameSite=Lax`;
    }, get: function () {
        const name = ".AspNetCore.Culture=";
        const decodedCookie = decodeURIComponent(document.cookie);
        const cookies = decodedCookie.split(';');
        for (let i = 0; i < cookies.length; i++) {
            let c = cookies[i].trim();
            if (c.indexOf(name) === 0) {
                const value = c.substring(name.length);
                const cPart = value.split('|')[0];
                return cPart.split('=')[1];
            }
        }
        return null;
    }
};
//Aktivieren Sie die Funktion beim Ändern der Größe
window.addEventListener('resize', window.updateHeaderHeight);
// select text in input
window.selectTextById = (id) => {
    const el = document.getElementById(id);
    if (el && el.select) {
        el.select();
    }
};
// script wenn Handy dann wird Whatsapp geöffent sonst wird webseite von Whatsapp geöffenet
window.whatsappRedirect = {
    openWhatsAppWithoutNumber: function(message) {
        const text = encodeURIComponent(message);
        const url = `https://api.whatsapp.com/send?text=${text}`;

        // Gerättype prüfen
        const isMobile = /Android|iPhone|iPad|iPod|Opera Mini|IEMobile|WPDesktop/i.test(navigator.userAgent);

        if (isMobile) {
            // auf Handy versuche erst app zu öffnen
            window.location = url;
        } else {
            // auf Computer app nicht möglich, öffne Web
            window.open(url, '_blank');
        }
    },
openWhatsApp: function(phone, message) {
        const number = String(phone).replace(/\D/g, '');
        const text = message ? encodeURIComponent(message) : '';

        // linke
        const appUrl = text
            ? `whatsapp://send?phone=${number}&text=${text}`
            : `whatsapp://send?phone=${number}`;
        const webUrl = text
            ? `https://api.whatsapp.com/send?phone=${number}&text=${text}`
            : `https://wa.me/${number}`;

        // Gerättype prüfen
        const isMobile = /Android|iPhone|iPad|iPod|Opera Mini|IEMobile|WPDesktop/i.test(navigator.userAgent);

        if (isMobile) {
            // auf Handy versuche erst app zu öffnen
            window.location = appUrl;
        } else {
            // auf Computer app nicht möglich, öffne Web
            window.open(webUrl, '_blank');
        }
    }
};
// Scroll sperren
window.openFullscreen = function () {
    document.body.style.overflow = 'hidden';
};
// Code zum Wiederverbinden nach der Rückkehr von WhatsApp oder aus dem Hintergrund
window.addEventListener('focus', async () => {
    try {
        // Versuch der manuellen Wiederverbindung
        await Blazor.reconnect();
    } catch (e) {
        console.log("Reconnection attempt failed, but Blazor will keep trying...");
    }
});
// OnMap
window.mapRedirect = {
    openMap: function(latitude, longitude, address = '') {
        latitude = String(latitude).trim();
        longitude = String(longitude).trim();
        address = String(address || '').trim();

        // Wenn ein Name vorhanden ist, verwenden wir diesen nur als Suchanfrage; andernfalls verwenden wir die Koordinaten.
        const query = address ? encodeURIComponent(address) : `${latitude},${longitude}`;

        // Links basierend auf Namen oder Koordinaten
        const appleUrl = `https://maps.apple.com/?q=${query}&ll=${latitude},${longitude}`;
        const googleUrl = `https://www.google.com/maps/search/?api=1&query=${query}`;
        const webUrl = `https://www.google.com/maps/search/?api=1&query=${query}`;

        const ua = navigator.userAgent || window.opera;
        const isIOS = /iPad|iPhone|iPod/.test(ua) && !window.MSStream;
        const isAndroid = /Android/.test(ua);

        const openWebFallback = () => window.open(webUrl, '_blank');

        if (isIOS) {
            document.getElementById('appleMapsBtn').onclick = () => {
                window.open(appleUrl, '_blank');
                window.mapRedirect.closeModal();
            };

            document.getElementById('googleMapsBtn').onclick = () => {
                window.open(googleUrl, '_blank');
                window.mapRedirect.closeModal();
            };

          
            document.getElementById('customMapModal').style.display = 'flex';

        } else if (isAndroid) {
            const googleUniversalUrl = `https://www.google.com/maps/search/?api=1&query=${query}`;
            window.open(googleUniversalUrl, '_blank');
        } else {
            //Jedes andere Gerät → Web direkt öffnen
            openWebFallback();
        }
    },
    closeModal: function () {
        document.getElementById('customMapModal').style.display = 'none';
    }
};
// OnPhone
window.phoneRedirect = {
    openPhoneDialer: function(phone) {
        const number = String(phone).replace(/[^\d+]/g, '');

        if (number) {
            window.location.href = `tel:${number}`;
        }
    }
};
// barcode scannen
var html5QrCode;
window.startLiveScanner = (dotNetHelper) => {
    // Erstelle das Objekt und verknüpfe es mit dem Element mit der ID "reader"
    html5QrCode = new Html5Qrcode("reader");

    const config = {
        fps: 10,
        qrbox: { width: 250, height: 150 } //Definition des Untersuchungsgebiets
    };

    html5QrCode.start(
        { facingMode: "environment" }, // Rückfahrkamera
        config,
        (decodedText) => {
            // Wenn die Messung erfolgreich ist, senden wir das Ergebnis an Bledzor.
            dotNetHelper.invokeMethodAsync('OnBarcodeScanned', decodedText);
            window.stopLiveScanner(); // Schalten Sie die Kamera nach dem Lesen aus.
        },
        (errorMessage) => { /* Die Suche wird fortgesetzt... */ }
    ).catch(err => {
        console.error("Unable to start scanning.", err);
    });
};

window.stopLiveScanner = () => {
    if (html5QrCode && html5QrCode.isScanning) {
        html5QrCode.stop().then(() => {
            html5QrCode.clear();
        });
    }
};
