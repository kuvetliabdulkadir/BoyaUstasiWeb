// Yardımcı Fonksiyonlar

// Telefon formatlama (Input Mask)
function formatPhoneNumberInput(input) {
    let value = input.value.replace(/\D/g, '');
    // Maksimum 11 hane
    if (value.length > 11) value = value.slice(0, 11);

    // Format uygula
    if (value.length > 7) {
        // 0xxx xxx xxxx
        value = value.slice(0, 4) + ' ' + value.slice(4, 7) + ' ' + value.slice(7);
    } else if (value.length > 4) {
        // 0xxx xxx
        value = value.slice(0, 4) + ' ' + value.slice(4);
    }

    input.value = value;
}
