// İletişim Formu

// Global değişkenler
let cityDistrictsMap = {};
let defaultCity = '';
let preselectedServiceType = null;
let preselectedDistrict = null;
let turnstileSiteKey = null;
let shouldShowCaptcha = false;

// Turnstile yönetimi
window.turnstileToken = null;
window.turnstileWidgetId = null;

// Widget render et
function initTurnstile() {
    const widgetElement = document.getElementById('turnstile-widget');
    if (!widgetElement || !turnstileSiteKey) {
        return;
    }

    if (typeof turnstile === 'undefined') {
        return;
    }

    try {
        // Mevcut widget'ı temizle
        if (window.turnstileWidgetId !== null) {
            try {
                turnstile.remove(window.turnstileWidgetId);
            } catch (e) {
                // Hata yoksay
            }
            window.turnstileWidgetId = null;
        }

        // İçeriği temizle
        widgetElement.innerHTML = '';

        // Yeni widget oluştur
        window.turnstileWidgetId = turnstile.render(widgetElement, {
            sitekey: turnstileSiteKey,
            theme: 'auto',
            size: 'normal',
            'appearance': 'always',
            callback: function (token) {
                window.turnstileToken = token;
                onSuccessTurnstileCallback(token);
            },
            'error-callback': onErrorTurnstileCallback,
            'expired-callback': onExpireTurnstileCallback
        });
    } catch (error) {
        widgetElement.innerHTML = '<div class="alert alert-danger"><i class="bi bi-exclamation-triangle me-2"></i>Güvenlik doğrulaması yüklenemedi. Lütfen sayfayı yenileyin.</div>';
    }
}

// Yükleme geri çağırımı
window.onloadTurnstileCallback = function () {
    setTimeout(initTurnstile, 100);
};

window.onSuccessTurnstileCallback = function (token) {
    window.turnstileToken = token;
    const captchaError = document.getElementById('captcha-error');
    if (captchaError) {
        captchaError.style.display = 'none';
    }
};

// Sayfa önbellek temizliği
window.addEventListener('pageshow', function (event) {
    // Önbellek kontrolü
    if (event.persisted || (performance.navigation && performance.navigation.type === 2)) {
        const contactForm = document.getElementById('contactForm');
        if (contactForm) {
            // Formu sıfırla
            contactForm.reset();

            // Token temizle
            window.turnstileToken = null;

            // Widget reset
            if (window.turnstileWidgetId !== null && typeof turnstile !== 'undefined') {
                try {
                    turnstile.reset(window.turnstileWidgetId);
                } catch (e) {
                    // Hata yoksay
                }
            }

            // Gizli token temizle
            const turnstileResponse = document.querySelector('input[name="cf-turnstile-response"]');
            if (turnstileResponse) {
                turnstileResponse.value = '';
            }

            // Yükleme zamanı
            const formLoadTimeInput = document.getElementById('formLoadTime');
            if (formLoadTimeInput) {
                const formLoadTime = Math.floor(Date.now() / 1000);
                formLoadTimeInput.value = formLoadTime.toString();
                contactForm.setAttribute('data-form-load-time', formLoadTime.toString());
            }

            // Dropdown sıfırla
            const citySelect = document.getElementById('citySelect');
            const districtSelect = document.getElementById('districtSelect');
            if (citySelect && defaultCity) {
                citySelect.value = defaultCity;
                // İlçeleri güncelle
                if (typeof updateDistricts === 'function') {
                    updateDistricts();
                }
            }
            if (districtSelect) {
                districtSelect.value = '';
                districtSelect.disabled = true;
            }

            // Önizleme temizle
            const filePreview = document.getElementById('filePreview');
            if (filePreview) {
                filePreview.innerHTML = '';
            }

            // Sayaç sıfırla
            const messageTextarea = document.getElementById('messageTextarea');
            const charCount = document.getElementById('charCount');
            if (messageTextarea && charCount) {
                charCount.textContent = '0/2000';
                charCount.style.color = '';
            }

            // Hata mesajlarını temizle
            const errorMessages = contactForm.querySelectorAll('.text-danger, .is-invalid');
            errorMessages.forEach(el => {
                el.classList.remove('is-invalid');
                if (el.tagName === 'SPAN' || el.tagName === 'DIV') {
                    el.textContent = '';
                }
            });
        }
    }
});

window.onErrorTurnstileCallback = function () {
    const captchaError = document.getElementById('captcha-error');
    if (captchaError) {
        captchaError.style.display = 'block';
        captchaError.querySelector('span').textContent = 'Güvenlik doğrulaması sırasında bir hata oluştu. Lütfen sayfayı yenileyin.';
    }
};

window.onExpireTurnstileCallback = function () {
    window.turnstileToken = null;
    const captchaError = document.getElementById('captcha-error');
    if (captchaError) {
        captchaError.style.display = 'block';
        captchaError.querySelector('span').textContent = 'Güvenlik doğrulaması süresi doldu. Lütfen tekrar tamamlayın.';
    }
};

// İl-İlçe yönetimi
function updateDistricts() {
    const citySelect = document.getElementById('citySelect');
    const districtSelect = document.getElementById('districtSelect');

    if (!citySelect || !districtSelect) return;

    const selectedCity = citySelect.value;
    districtSelect.innerHTML = '<option value="">Seçiniz</option>';

    if (!selectedCity) {
        districtSelect.disabled = true;
        districtSelect.innerHTML = '<option value="">Önce il seçiniz</option>';
        return;
    }

    districtSelect.disabled = false;
    const districts = cityDistrictsMap[selectedCity] || [];

    districts.forEach(function (district) {
        const option = document.createElement('option');
        option.value = district;
        option.textContent = district;
        districtSelect.appendChild(option);
    });

    // Varsayılan ilçe seçimi
    if (preselectedDistrict && districts.includes(preselectedDistrict)) {
        setTimeout(function () {
            districtSelect.value = preselectedDistrict;
        }, 100);
    }
}

// Hizmet türü seçimi
function setupServiceTypeSelection() {
    const serviceTypeSelect = document.querySelector('select[name="FormModel.ServiceType"]');
    if (!serviceTypeSelect) return;

    // Varsayılan kontrolü
    let serviceTypeToSelect = preselectedServiceType;
    if (!serviceTypeToSelect) {
        // URL parametre kontrolü
        const urlParams = new URLSearchParams(window.location.search);
        serviceTypeToSelect = urlParams.get('serviceType');
    }

    // Değeri seç
    if (serviceTypeToSelect) {
        const trimmedValue = serviceTypeToSelect.trim();
        const options = Array.from(serviceTypeSelect.options);
        const matchingOption = options.find(opt => opt.value.trim() === trimmedValue);

        if (matchingOption) {
            serviceTypeSelect.value = matchingOption.value;
        }
    }
}

// İsim doğrulama
function setupFullNameValidation() {
    const fullNameInput = document.querySelector('input[name="FullName"]');
    if (!fullNameInput) return;

    fullNameInput.addEventListener('input', function () {
        // Karakter kısıtı
        this.value = this.value.replace(/[^a-zA-ZğüşıöçĞÜŞİÖÇ\s]/g, '');
    });

    fullNameInput.addEventListener('keypress', function (e) {
        // Tuş engelleme
        const char = String.fromCharCode(e.which);
        if (!/[a-zA-ZğüşıöçĞÜŞİÖÇ\s]/.test(char)) {
            e.preventDefault();
        }
    });

    // Yapıştırma kontrolü
    fullNameInput.addEventListener('paste', function (e) {
        e.preventDefault();
        const pastedText = (e.clipboardData || window.clipboardData).getData('text');
        const cleanedText = pastedText.replace(/[^a-zA-ZğüşıöçĞÜŞİÖÇ\s]/g, '');
        this.value = cleanedText;
    });
}

// Sayısal girdi doğrulama
function setupNumericInput(input, min, max, maxLength = 10) {
    if (!input) return;

    input.addEventListener('input', function () {
        let value = this.value.replace(/[^0-9]/g, '');
        if (value.length > maxLength) value = value.substring(0, maxLength);
        this.value = value;

        if (value.length > 0) {
            let numValue = parseInt(value);
            if (numValue < min) this.value = min.toString();
            else if (numValue > max) this.value = max.toString();
        }
    });

    input.addEventListener('keypress', function (e) {
        const char = String.fromCharCode(e.which);
        if (!/[0-9]/.test(char)) {
            e.preventDefault();
            return;
        }
        if (this.value.length >= maxLength) e.preventDefault();
    });

    input.addEventListener('paste', function (e) {
        e.preventDefault();
        const pastedText = (e.clipboardData || window.clipboardData).getData('text');
        const cleanedText = pastedText.replace(/[^0-9]/g, '').substring(0, maxLength);
        this.value = cleanedText;
    });

    input.addEventListener('blur', function () {
        let value = parseInt(this.value);
        if (isNaN(value) || value < min) this.value = '';
        else if (value > max) this.value = max.toString();
    });
}

// Metrekare doğrulama
function setupSquareMetersValidation() {
    const minSquareMetersInput = document.querySelector('input[name="MinSquareMeters"]');
    const maxSquareMetersInput = document.querySelector('input[name="MaxSquareMeters"]');

    // Min-Max ayarları
    setupNumericInput(minSquareMetersInput, 1, 99999);
    setupNumericInput(maxSquareMetersInput, 1, 100000);

    // Min-Max kontrolü
    if (minSquareMetersInput && maxSquareMetersInput) {
        function validateMinMax() {
            const minValue = parseInt(minSquareMetersInput.value);
            const maxValue = parseInt(maxSquareMetersInput.value);

            if (!isNaN(minValue) && !isNaN(maxValue)) {
                if (minValue > maxValue) {
                    maxSquareMetersInput.setCustomValidity('Maksimum metrekare, minimum metrekareden küçük olamaz.');
                } else {
                    maxSquareMetersInput.setCustomValidity('');
                }
            } else {
                maxSquareMetersInput.setCustomValidity('');
            }
        }

        minSquareMetersInput.addEventListener('input', validateMinMax);
        maxSquareMetersInput.addEventListener('input', validateMinMax);
    }
}

// Karakter sayacı
function setupMessageCharCounter() {
    const messageTextarea = document.getElementById('messageTextarea');
    const charCount = document.getElementById('messageCharCount');

    if (!messageTextarea || !charCount) return;

    // Başlangıç sayacı
    charCount.textContent = messageTextarea.value.length;

    // Sayaç güncelle
    messageTextarea.addEventListener('input', function () {
        const length = this.value.length;
        charCount.textContent = length;

        // Limit uyarısı
        if (length >= 1900) {
            charCount.style.color = '#dc3545';
        } else if (length >= 1500) {
            charCount.style.color = '#ffc107';
        } else {
            charCount.style.color = '';
        }
    });
}

// Spam koruması
function setupSpamProtection() {
    const contactForm = document.getElementById('contactForm');
    const honeypotField = document.getElementById('website');
    const formLoadTimeInput = document.getElementById('formLoadTime');

    if (!contactForm) return;

    // Zaman kaydı
    const formLoadTime = Math.floor(Date.now() / 1000);
    if (formLoadTimeInput) {
        formLoadTimeInput.value = formLoadTime.toString();
    }
    contactForm.setAttribute('data-form-load-time', formLoadTime.toString());

    // Honeypot gizle
    if (honeypotField) {
        honeypotField.style.display = 'none';
        honeypotField.style.visibility = 'hidden';
        honeypotField.style.position = 'absolute';
        honeypotField.style.left = '-9999px';
        honeypotField.setAttribute('tabindex', '-1');
        honeypotField.setAttribute('autocomplete', 'off');
    }
}

// Dosya yükleme kontrolü
function setupFileUpload() {
    const attachmentsInput = document.getElementById('attachmentsInput');
    const filePreview = document.getElementById('filePreview');
    const fileError = document.getElementById('fileError');
    const maxFiles = 5;
    const maxFileSize = 5 * 1024 * 1024; // 5MB

    if (!attachmentsInput) return;

    attachmentsInput.addEventListener('change', function (e) {
        const files = Array.from(this.files);
        if (fileError) {
            fileError.style.display = 'none';
            fileError.textContent = '';
        }

        // Dosya sayısı limiti
        if (files.length > maxFiles) {
            if (fileError) {
                fileError.textContent = `Maksimum ${maxFiles} adet dosya seçebilirsiniz. İlk ${maxFiles} dosya seçildi.`;
                fileError.style.display = 'block';
            }

            // Limit uygula
            const dt = new DataTransfer();
            files.slice(0, maxFiles).forEach(file => dt.items.add(file));
            this.files = dt.files;
        }

        // Boyut limiti
        const validFiles = [];
        const invalidFiles = [];

        Array.from(this.files).forEach(file => {
            if (file.size > maxFileSize) {
                invalidFiles.push(file.name);
            } else {
                validFiles.push(file);
            }
        });

        if (invalidFiles.length > 0) {
            if (fileError) {
                fileError.textContent = `Şu dosyalar çok büyük (maksimum 5MB): ${invalidFiles.join(', ')}`;
                fileError.style.display = 'block';
            }

            // Hatalı dosyaları sil
            const dt = new DataTransfer();
            validFiles.forEach(file => dt.items.add(file));
            this.files = dt.files;
        }

        // Önizleme güncelle
        updateFilePreview();
    });

    function updateFilePreview() {
        if (!filePreview) return;

        const files = Array.from(attachmentsInput.files);
        filePreview.innerHTML = '';

        if (files.length === 0) {
            return;
        }

        const previewContainer = document.createElement('div');
        previewContainer.className = 'row g-2';

        files.forEach((file, index) => {
            const col = document.createElement('div');
            col.className = 'col-md-4 col-6';

            const card = document.createElement('div');
            card.className = 'card';
            card.style.position = 'relative';

            const img = document.createElement('img');
            img.className = 'card-img-top';
            img.style.height = '100px';
            img.style.objectFit = 'cover';
            img.style.cursor = 'pointer';

            const reader = new FileReader();
            reader.onload = function (e) {
                img.src = e.target.result;
            };
            reader.readAsDataURL(file);

            const cardBody = document.createElement('div');
            cardBody.className = 'card-body p-2';

            const fileName = document.createElement('small');
            fileName.className = 'text-muted d-block text-truncate';
            fileName.textContent = file.name;
            fileName.style.maxWidth = '100%';

            const fileSize = document.createElement('small');
            fileSize.className = 'text-muted d-block';
            fileSize.textContent = (file.size / 1024 / 1024).toFixed(2) + ' MB';

            cardBody.appendChild(fileName);
            cardBody.appendChild(fileSize);

            card.appendChild(img);
            card.appendChild(cardBody);
            col.appendChild(card);
            previewContainer.appendChild(col);
        });

        filePreview.appendChild(previewContainer);

        // Sayı bilgisi
        const fileCount = document.createElement('div');
        fileCount.className = 'mt-2 text-muted small';
        fileCount.textContent = `${files.length} / ${maxFiles} dosya seçildi`;
        filePreview.appendChild(fileCount);
    }
}

// Ana başlatıcı
function initContactForm() {
    // Veri öznitelikleri
    const contactPage = document.getElementById('contact-page');
    if (contactPage) {
        const cityDistrictsMapData = contactPage.getAttribute('data-city-districts-map');
        const defaultCityData = contactPage.getAttribute('data-default-city');
        const preselectedServiceTypeData = contactPage.getAttribute('data-preselected-service-type');
        const preselectedDistrictData = contactPage.getAttribute('data-preselected-district');
        const turnstileSiteKeyData = contactPage.getAttribute('data-turnstile-site-key');
        const shouldShowCaptchaData = contactPage.getAttribute('data-should-show-captcha');

        if (cityDistrictsMapData) {
            try {
                cityDistrictsMap = JSON.parse(cityDistrictsMapData);
            } catch (e) {
                // Parse hatası kontrolü
            }
        }

        if (defaultCityData) {
            defaultCity = defaultCityData;
        }

        if (preselectedServiceTypeData) {
            try {
                // JSON ayrıştır
                preselectedServiceType = JSON.parse(preselectedServiceTypeData);
            } catch (e) {
                // Parse hatası kontrolü
                preselectedServiceType = preselectedServiceTypeData === 'null' || preselectedServiceTypeData === '""' ? null : preselectedServiceTypeData;
            }
        }

        if (preselectedDistrictData) {
            preselectedDistrict = preselectedDistrictData === 'null' ? null : preselectedDistrictData;
        }

        if (turnstileSiteKeyData) {
            turnstileSiteKey = turnstileSiteKeyData;
        }

        if (shouldShowCaptchaData) {
            shouldShowCaptcha = shouldShowCaptchaData === 'true';
        }
    }

    // İl-İlçe bağlantısı
    const citySelect = document.getElementById('citySelect');
    const districtSelect = document.getElementById('districtSelect');

    if (citySelect && districtSelect) {
        // İl değişim olayı
        citySelect.addEventListener('change', function () {
            updateDistricts();
            // İlçe sıfırla
            districtSelect.value = '';
        });

        // Varsayılan yükleme
        if (citySelect.value) {
            updateDistricts();
        } else {
            districtSelect.disabled = true;
        }
    }

    // Yardımcıları başlat
    setupServiceTypeSelection();
    setupFullNameValidation();
    setupSquareMetersValidation();
    setupSquareMetersLimits(); // Min-max sınır kontrolü
    setupMessageCharCounter();
    setupSpamProtection();
    setupFileUpload();
    setupFormValidation(); // Form doğrulama
}

// Form doğrulama
function setupFormValidation() {
    const contactForm = document.getElementById('contactForm');
    if (!contactForm) return;

    // Gönderim olayı
    contactForm.addEventListener('submit', function (e) {
        let hasErrors = false;

        // Zorunlu alanlar
        const requiredFields = [
            { id: 'FormModel_FullName', name: 'Ad Soyad' },
            { id: 'FormModel_Phone', name: 'Telefon' },
            { id: 'citySelect', name: 'İl' },
            { id: 'districtSelect', name: 'İlçe' },
            { id: 'FormModel_ServiceType', name: 'Hizmet Türü' },
            { id: 'messageTextarea', name: 'Açıklama' }
        ];

        // Hataları temizle
        const existingErrors = contactForm.querySelectorAll('.validation-error');
        existingErrors.forEach(el => el.remove());

        requiredFields.forEach(field => {
            const input = document.getElementById(field.id);
            if (!input) return;

            const value = input.value ? input.value.trim() : '';
            let isValid = true;

            // Özel kurallar
            if (field.id === 'messageTextarea' && value.length < 10) {
                isValid = false;
            } else if (field.id === 'districtSelect' && input.disabled) {
                // İlçe durumu
                isValid = false;
            } else if (value === '' || value === 'Seçiniz' || value === 'Önce il seçiniz') {
                isValid = false;
            }

            if (!isValid) {
                hasErrors = true;

                // Hata göster
                const errorDiv = document.createElement('div');
                errorDiv.className = 'validation-error text-danger small mt-1';
                errorDiv.textContent = `${field.name} alanı zorunludur${field.id === 'messageTextarea' ? ' (en az 10 karakter)' : ''}.`;

                // Ebeveyn elemente ekle
                const parent = input.closest('.form-group') || input.parentElement;
                if (parent) {
                    parent.appendChild(errorDiv);
                }

                // Hata sınıfı ekle
                input.classList.add('is-invalid');
            } else {
                input.classList.remove('is-invalid');
            }
        });

        // E-posta formatı
        const emailInput = document.getElementById('FormModel_Email');
        if (emailInput) {
            const emailValue = emailInput.value ? emailInput.value.trim() : '';
            if (emailValue !== '') {
                // Regex kontrolü
                const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
                if (!emailRegex.test(emailValue)) {
                    hasErrors = true;

                    // Hata göster
                    const errorDiv = document.createElement('div');
                    errorDiv.className = 'validation-error text-danger small mt-1';
                    errorDiv.textContent = 'Geçerli bir e-posta adresi giriniz.';

                    // Ebeveyn elemente ekle
                    const parent = emailInput.closest('.form-group') || emailInput.parentElement;
                    if (parent) {
                        parent.appendChild(errorDiv);
                    }

                    // Hata sınıfı ekle
                    emailInput.classList.add('is-invalid');
                } else {
                    emailInput.classList.remove('is-invalid');
                }
            }
        }

        if (hasErrors) {
            e.preventDefault();

            // Hataya kaydır
            const firstError = contactForm.querySelector('.is-invalid');
            if (firstError) {
                firstError.scrollIntoView({ behavior: 'smooth', block: 'center' });
                firstError.focus();
            }

            // Uyarı göster
            const alertDiv = document.createElement('div');
            alertDiv.className = 'alert alert-warning alert-dismissible fade show position-fixed top-0 start-50 translate-middle-x mt-3';
            alertDiv.style.zIndex = '9999';
            alertDiv.style.maxWidth = '500px';
            alertDiv.innerHTML = `
                <i class="bi bi-exclamation-triangle me-2"></i>
                <strong>Lütfen Tüm Zorunlu Alanları Doldurun!</strong>
                <p class="mb-0 mt-2">Formu göndermek için tüm zorunlu alanları doldurmanız gerekmektedir.</p>
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            `;
            document.body.appendChild(alertDiv);

            // Otomatik kapat
            setTimeout(() => {
                if (alertDiv.parentNode) {
                    alertDiv.remove();
                }
            }, 5000);

            return false;
        }
    });
}

// Metrekare limit kontrolü
function setupSquareMetersLimits() {
    const minSquareMetersInput = document.getElementById('FormModel_MinSquareMeters');
    const maxSquareMetersInput = document.getElementById('FormModel_MaxSquareMeters');

    // Min sınır
    if (minSquareMetersInput) {
        minSquareMetersInput.addEventListener('input', function () {
            let value = parseInt(this.value);
            if (isNaN(value)) return;

            if (value < 1) {
                this.value = 1;
            } else if (value > 99999) {
                this.value = 99999;
            }
        });

        minSquareMetersInput.addEventListener('blur', function () {
            let value = parseInt(this.value);
            if (isNaN(value)) return;

            if (value < 1) {
                this.value = 1;
            } else if (value > 99999) {
                this.value = 99999;
            }
        });
    }

    // Max sınır
    if (maxSquareMetersInput) {
        maxSquareMetersInput.addEventListener('input', function () {
            let value = parseInt(this.value);
            if (isNaN(value)) return;

            if (value < 1) {
                this.value = 1;
            } else if (value > 100000) {
                this.value = 100000;
            }
        });

        maxSquareMetersInput.addEventListener('blur', function () {
            let value = parseInt(this.value);
            if (isNaN(value)) return;

            if (value < 1) {
                this.value = 1;
            } else if (value > 100000) {
                this.value = 100000;
            }
        });
    }
}

// DOM Hazır
document.addEventListener('DOMContentLoaded', initContactForm);

