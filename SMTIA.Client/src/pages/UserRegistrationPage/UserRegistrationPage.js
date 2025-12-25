import React, { useState } from 'react';
import './UserRegistrationPage.css';
import KVKKPage from '../KVKKPage/KVKKPage';
import AgeSelectionPage from '../AgeSelectionPage/AgeSelectionPage';
import logo from '../../assets/logos/logo.png';
import { smtiaApi } from '../../services/smtiaApi';

const UserRegistrationPage = ({ onRegistrationComplete, onBackToIntro }) => {
  const [currentStep, setCurrentStep] = useState(1); 
  const [isTransitioning, setIsTransitioning] = useState(false);
  const [errorMessage, setErrorMessage] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [showKVKKMessage, setShowKVKKMessage] = useState(false);
  const [showKVKKPage, setShowKVKKPage] = useState(false);
  const [showAgeSelection, setShowAgeSelection] = useState(false);
  const [showBackTooltip, setShowBackTooltip] = useState(false);
  const [isCheckingEmail, setIsCheckingEmail] = useState(false);
  const [formData, setFormData] = useState({
    name: '',
    surname: '',
    email: '',
    password: ''
  });

  const steps = [
    { id: 1, title: 'Adınız', placeholder: 'Adınızı girin', field: 'name' },
    { id: 2, title: 'Soyadınız', placeholder: 'Soyadınızı girin', field: 'surname' },
    { id: 3, title: 'E-posta Adresiniz', placeholder: 'E-posta adresinizi girin', field: 'email' },
    { id: 4, title: 'Şifrenizi Oluşturun', placeholder: 'Şifrenizi oluşturun', field: 'password' }
  ];


  const getProgressIndicator = () => {
    return '1/6'; 
  };


  const validateName = (name) => {
    if (!name.trim()) {
      return 'Ad alanı boş olamaz';
    }
    if (name.trim().length < 2) {
      return 'Ad en az 2 karakter olmalıdır';
    }
    if (!/^[a-zA-ZğüşıöçĞÜŞİÖÇ\s]+$/.test(name.trim())) {
      return 'Ad sadece harflerden oluşmalıdır';
    }
    return null;
  };

  const validateSurname = (surname) => {
    if (!surname.trim()) {
      return 'Soyad alanı boş olamaz';
    }
    if (surname.trim().length < 2) {
      return 'Soyad en az 2 karakter olmalıdır';
    }
    if (!/^[a-zA-ZğüşıöçĞÜŞİÖÇ\s]+$/.test(surname.trim())) {
      return 'Soyad sadece harflerden oluşmalıdır';
    }
    return null;
  };

  const validateEmail = (email) => {
    if (!email.trim()) {
      return 'E-posta alanı boş olamaz';
    }
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(email.trim())) {
      return 'Geçerli bir e-posta adresi giriniz';
    }
    return null;
  };

  const validatePassword = (password) => {
    if (!password.trim()) {
      return 'Şifre alanı boş olamaz';
    }
    if (password.length < 8) {
      return 'Şifre en az 8 karakter olmalıdır';
    }
    if (!/(?=.*[a-z])/.test(password)) {
      return 'Şifre en az 1 küçük harf içermelidir';
    }
    if (!/(?=.*[A-Z])/.test(password)) {
      return 'Şifre en az 1 büyük harf içermelidir';
    }
    if (!/(?=.*\d)/.test(password)) {
      return 'Şifre en az 1 sayı içermelidir';
    }
    if (!/(?=.*[!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?])/.test(password)) {
      return 'Şifre en az 1 özel karakter içermelidir';
    }
    return null;
  };

  const currentStepData = steps.find(step => step.id === currentStep);
  const currentValue = formData[currentStepData.field];

  const handleNext = () => {
    if (!currentValue.trim()) {
      setErrorMessage('Bu alan boş olamaz');
      return;
    }

    let validationError = null;
    
    switch (currentStep) {
      case 1:
        validationError = validateName(currentValue);
        break;
      case 2:
        validationError = validateSurname(currentValue);
        break;
      case 3:
        validationError = validateEmail(currentValue);
        break;
      case 4:
        validationError = validatePassword(currentValue);
        break;
      default:
        break;
    }

    if (validationError) {
      setErrorMessage(validationError);
      return;
    }

    
    setErrorMessage('');

    
    if (currentStep === 3) {
      setIsCheckingEmail(true);
      
      const checkEmailAvailability = async () => {
        try {
          const res = await smtiaApi.public.isEmailAvailable(currentValue.trim());
          setIsCheckingEmail(false);

          if (res && res.available === false) {
            setErrorMessage('Bu e-posta adresi zaten kullanılıyor');
            return;
          }

          setIsTransitioning(true);
          setTimeout(() => {
            setCurrentStep(currentStep + 1);
            setIsTransitioning(false);
          }, 200);
        } catch (error) {
          console.error('Email check API call failed:', error);
          setIsCheckingEmail(false);
          setErrorMessage(error?.message || 'E-posta kontrolü başarısız oldu. Lütfen tekrar deneyin.');
        }
      };
      
      checkEmailAvailability();
      return;
    }

    if (currentStep < 4) {
      setIsTransitioning(true);
      setTimeout(() => {
        setCurrentStep(currentStep + 1);
        setIsTransitioning(false);
      }, 200);
    } else {
      setIsTransitioning(true);
      setTimeout(() => {
        setShowAgeSelection(true);
      }, 300);
    }
  };

  const handleBack = () => {
    if (currentStep > 1) {
      setIsTransitioning(true);
      setTimeout(() => {
        setCurrentStep(currentStep - 1);
        setIsTransitioning(false);
        setErrorMessage(''); 
      }, 200);
    } else {
      onBackToIntro();
    }
  };

  const handleInputChange = (e) => {
    setFormData({
      ...formData,
      [currentStepData.field]: e.target.value
    });
    if (errorMessage) {
      setErrorMessage('');
    }
  };

  const handleKeyPress = (e) => {
    if (e.key === 'Enter' && currentValue.trim() && !isTransitioning) {
      handleNext();
    }
  };

  const handleKeyDown = (e) => {
    if (e.key === 'Escape') {
      handleBack();
    }
  };

  const getInputType = () => {
    if (currentStep === 3) return 'email';
    if (currentStep === 4) return showPassword ? 'text' : 'password';
    return 'text';
  };

  const togglePasswordVisibility = () => {
    setShowPassword(!showPassword);
  };

  const getPasswordRequirements = () => {
    const password = currentValue;
    const requirements = [
      {
        text: 'En az 1 büyük harf',
        met: /(?=.*[A-Z])/.test(password)
      },
      {
        text: 'En az 1 küçük harf',
        met: /(?=.*[a-z])/.test(password)
      },
      {
        text: 'En az 1 sayı',
        met: /(?=.*\d)/.test(password)
      },
      {
        text: 'En az 1 özel karakter',
        met: /(?=.*[!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?])/.test(password)
      },
      {
        text: 'En az 8 karakter',
        met: password.length >= 8
      }
    ];
    return requirements;
  };

  const getCurrentRequirement = () => {
    const requirements = getPasswordRequirements();
    const nextUnmet = requirements.find(req => !req.met);
    if (!nextUnmet) {
      return requirements[requirements.length - 1];
    }
    return nextUnmet;
  };

  const getCompletedRequirements = () => {
    const requirements = getPasswordRequirements();
    return requirements.filter(req => req.met);
  };

  const isPasswordValid = () => {
    const password = currentValue;
    return password.length >= 8 && 
           /(?=.*[A-Z])/.test(password) && 
           /(?=.*[a-z])/.test(password) && 
           /(?=.*\d)/.test(password) && 
           /(?=.*[!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?])/.test(password);
  };

  React.useEffect(() => {
    const timer = setTimeout(() => {
      setShowKVKKMessage(true);
    }, 4000);

    return () => clearTimeout(timer);
  }, []);

  const handleKVKKClose = () => {
    setShowKVKKMessage(false);
  };

  const handleKVKKClick = () => {
    setShowKVKKPage(true);
  };

  const handleBackFromKVKK = () => {
    setShowKVKKPage(false);
  };

  const handleAgeSelected = (age) => {
    const completeFormData = {
      ...formData,
      age: age
    };
    onRegistrationComplete(completeFormData);
  };

  const handleKVKKCloseFromAgeSelection = () => {
    setShowKVKKMessage(false);
  };

  const handleBackFromAgeSelection = () => {
    setShowAgeSelection(false);
    setIsTransitioning(false);
  };

  const handleBackMouseEnter = () => {
    const timer = setTimeout(() => {
      setShowBackTooltip(true);
    }, 2000);
    
    handleBackMouseEnter.timer = timer;
  };

  const handleBackMouseLeave = () => {
    if (handleBackMouseEnter.timer) {
      clearTimeout(handleBackMouseEnter.timer);
    }
    setShowBackTooltip(false);
  };

  if (showKVKKPage) {
    return <KVKKPage onBack={handleBackFromKVKK} />;
  }

  if (showAgeSelection) {
    return <AgeSelectionPage 
      onAgeSelected={handleAgeSelected} 
      onBack={handleBackFromAgeSelection} 
      currentStep={2} 
      totalSteps={6}
      showKVKKMessage={showKVKKMessage}
      onKVKKClose={handleKVKKCloseFromAgeSelection}
      onKVKKClick={handleKVKKClick}
      userRegistrationData={formData}
    />;
  }

  return (
    <div className="user-registration-container">
      <div className="user-registration-header">
        <div className="user-registration-logo-section">
          <img src={logo} alt="Logo" className="user-registration-logo" />
          <span className="user-registration-group-text">
            <span className="user-registration-by-text">by</span> <span className="user-registration-group-name">Group X</span>
          </span>
        </div>
        <div className="user-registration-header-right">
          <div 
            className="user-registration-back-button" 
            onClick={handleBack}
            onMouseEnter={handleBackMouseEnter}
            onMouseLeave={handleBackMouseLeave}
          >
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
              <path d="M19 12H5"/>
              <path d="M12 19l-7-7 7-7"/>
            </svg>
            {showBackTooltip && (
              <span className="back-tooltip">Geri dön</span>
            )}
          </div>
          <div className="user-registration-progress-indicator">
            {getProgressIndicator()}
          </div>
        </div>
      </div>

      <div className="user-registration-content">
        <div className={`user-registration-title ${isTransitioning ? 'fade-out' : 'fade-in'}`}>
          <h1>{currentStepData.title}</h1>
        </div>

        <div className={`user-registration-input-container ${isTransitioning ? 'fade-out' : 'fade-in'}`}>
          <div className={`user-registration-input-wrapper ${errorMessage ? 'error' : ''}`}>
            <input
              type={getInputType()}
              placeholder={currentStepData.placeholder}
              value={currentValue}
              onChange={handleInputChange}
              onKeyPress={handleKeyPress}
              onKeyDown={handleKeyDown}
              className="user-registration-input"
              autoFocus
            />
            {currentStep === 4 && (
              <button
                type="button"
                className="user-registration-password-toggle"
                onClick={togglePasswordVisibility}
                title={showPassword ? 'Şifreyi gizle' : 'Şifreyi göster'}
              >
                {showPassword ? (
                  <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                    <path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19m-6.72-1.07a3 3 0 1 1-4.24-4.24"/>
                    <line x1="1" y1="1" x2="23" y2="23"/>
                  </svg>
                ) : (
                  <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                    <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/>
                    <circle cx="12" cy="12" r="3"/>
                  </svg>
                )}
              </button>
            )}
            {currentStep !== 4 && (
              <button 
                className={`user-registration-button user-registration-button-inline ${isTransitioning || isCheckingEmail ? 'disabled' : ''}`}
                onClick={handleNext}
                disabled={!currentValue.trim() || isTransitioning || isCheckingEmail}
              >
                {isCheckingEmail ? (
                  <div className="spinner"></div>
                ) : (
                  'Devam Et'
                )}
              </button>
            )}
          </div>
          
          {currentStep !== 4 && (
            <button 
              className={`user-registration-button user-registration-button-mobile ${isTransitioning || isCheckingEmail ? 'disabled' : ''}`}
              onClick={handleNext}
              disabled={!currentValue.trim() || isTransitioning || isCheckingEmail}
            >
              {isCheckingEmail ? (
                <div className="spinner"></div>
              ) : (
                'Devam Et'
              )}
            </button>
          )}
          {errorMessage && (
            <div className="user-registration-error-message">
              <div className="error-icon">
                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                  <circle cx="12" cy="12" r="10"/>
                  <line x1="12" y1="8" x2="12" y2="12"/>
                  <line x1="12" y1="16" x2="12.01" y2="16"/>
                </svg>
              </div>
              <div className="error-text">{errorMessage}</div>
            </div>
          )}
          
          {currentStep === 4 && currentValue.trim() && (
            <div className="user-registration-password-requirements">
              <div className="requirements-list">
                {getCompletedRequirements().map((requirement, index) => (
                  <div key={`completed-${index}`} className="requirement-item completed">
                    <div className="requirement-icon">
                      <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                        <polyline points="20,6 9,17 4,12"/>
                      </svg>
                    </div>
                    <span className="requirement-text">{requirement.text}</span>
                  </div>
                ))}
                
                {(() => {
                  const currentReq = getCurrentRequirement();
                  if (!currentReq.met) {
                    return (
                      <div className="requirement-item">
                        <div className="requirement-icon">
                          <div className="requirement-dot"></div>
                        </div>
                        <span className="requirement-text">{currentReq.text}</span>
                      </div>
                    );
                  }
                  return null;
                })()}
              </div>
            </div>
          )}
        </div>
      </div>

      {currentStep === 4 && currentValue.trim() && !errorMessage && isPasswordValid() && (
        <div className="user-registration-next-page-button" onClick={handleNext}>
          <span>Sonraki Sayfa</span>
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
            <path d="M5 12h14"/>
            <path d="M12 5l7 7-7 7"/>
          </svg>
        </div>
      )}

      {showKVKKMessage && (
        <div className="user-registration-kvkk-message">
          <div className="kvkk-content">
            <span className="kvkk-text">Gizliliğinizde önem veriyoruz. Daha fazla bilgi için Veri ve Çerez Politikasını ziyaret edebilirsiniz.</span>
            <button className="kvkk-button" onClick={handleKVKKClick}>KVKK</button>
          </div>
          <button className="kvkk-close" onClick={handleKVKKClose}>
            <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
              <line x1="18" y1="6" x2="6" y2="18"/>
              <line x1="6" y1="6" x2="18" y2="18"/>
            </svg>
          </button>
        </div>
      )}

    </div>
  );
};

export default UserRegistrationPage;
