import React, { useState, useEffect } from 'react';
import './AddCapsulePage.css';
import logo from '../../assets/logos/logo.png';
import AddMedicineModal from '../../components/AddMedicineModal/AddMedicineModal';
import AddMedicineScheduleModal from '../../components/AddMedicineScheduleModal/AddMedicineScheduleModal';
import capsuleType1 from '../../assets/images/capsule-type-1.png';
import capsuleType2 from '../../assets/images/capsule-type-2.png';
import capsuleType3 from '../../assets/images/capsule-type-3.png';
import capsuleType4 from '../../assets/images/capsule-type-4.png';
import { smtiaApi } from '../../services/smtiaApi';
import { setToken } from '../../services/auth';

const AddCapsulePage = ({ onBack, currentStep = 6, totalSteps = 6, showKVKKMessage = false, onKVKKClose, onKVKKClick, userRegistrationData = {} }) => {
  const [isTransitioning, setIsTransitioning] = useState(false);
  const [showBackTooltip, setShowBackTooltip] = useState(false);
  const [showModal, setShowModal] = useState(false);
  const [showScheduleModal, setShowScheduleModal] = useState(false);
  const [currentMedicineData, setCurrentMedicineData] = useState(null);
  const [addedMedicines, setAddedMedicines] = useState([]);
  const [isProcessing, setIsProcessing] = useState(false);
  const [showSuccessScreen, setShowSuccessScreen] = useState(false);
  const [registrationMessage, setRegistrationMessage] = useState('');

  const handleBack = () => {
    setIsTransitioning(true);
    setTimeout(() => {
      onBack();
    }, 200);
  };

  const handleAddCapsule = () => {
    if (isProcessing || isTransitioning || showModal || showScheduleModal) {
      return;
    }
    
    setIsProcessing(true);
    
    setTimeout(() => {
      setCurrentMedicineData(null);
      setShowModal(true);
      setIsProcessing(false);
    }, 50);
  };

  const handleModalClose = () => {
    if (isTransitioning || isProcessing) return;
    
    setIsTransitioning(true);
    setTimeout(() => {
      setShowModal(false);
      setIsTransitioning(false);
    }, 200);
  };

  const handleModalNext = (medicineData) => {
    if (isTransitioning || isProcessing) return;
    
    setCurrentMedicineData(medicineData);
    setIsTransitioning(true);
    setTimeout(() => {
      setShowModal(false);
      setIsTransitioning(false);
      setTimeout(() => {
        setShowScheduleModal(true);
      }, 100);
    }, 200);
  };

  const handleScheduleModalBack = () => {
    if (isTransitioning || isProcessing) return;
    
    setIsTransitioning(true);
    setTimeout(() => {
      setShowScheduleModal(false);
      setIsTransitioning(false);
      setTimeout(() => {
        setShowModal(true);
      }, 100);
    }, 200);
  };

  const handleScheduleModalClose = () => {
    if (isTransitioning || isProcessing) return;
    
    setIsTransitioning(true);
    setTimeout(() => {
      setShowScheduleModal(false);
      setIsTransitioning(false);
      setCurrentMedicineData(null);
    }, 200);
  };

  const handleAddMedicine = (completeMedicineData) => {
    if (isTransitioning || isProcessing) return;
    
    setIsTransitioning(true);
    setTimeout(() => {
      setAddedMedicines(prev => [...prev, completeMedicineData]);
      setShowScheduleModal(false);
      setIsTransitioning(false);
      setCurrentMedicineData(null);
    }, 200);
  };

  const handleCompleteRegistration = () => {
    if (isProcessing || isTransitioning) return;
    
    setIsProcessing(true);
    
    const email = (userRegistrationData.email || '').trim();
    const password = userRegistrationData.password || '';
    const firstName = (userRegistrationData.name || '').trim();
    const lastName = (userRegistrationData.surname || '').trim();
    const userName = email.includes('@') ? email.split('@')[0] : email;

    const ageYears = userRegistrationData.age ? Number(userRegistrationData.age) : null;
    const heightCm = userRegistrationData.height ? Number(userRegistrationData.height) : null;
    const weightKg = userRegistrationData.weight ? Number(userRegistrationData.weight) : null;
    const gender = userRegistrationData.gender || null;

    const medicines = (addedMedicines || []).map(m => ({
      name: m.name,
      type: m.type,
      doseAmount: Number(m.dose?.amount || 1),
      doseUnit: m.dose?.unit || 'adet',
      selectedDays: m.schedule?.selectedDays || [],
      times: (m.schedule?.times || []).map(t => ({ time: t.time, dosage: t.dosage })),
      note: m.notes || null,
      packageSize: 0
    }));

    const registerPayload = {
      firstName,
      lastName,
      email,
      password,
      userName: userName || `${firstName}.${lastName}`.replace(/\s+/g, '').toLowerCase(),
      ageYears: Number.isFinite(ageYears) ? ageYears : null,
      heightCm: Number.isFinite(heightCm) ? heightCm : null,
      weightKg: Number.isFinite(weightKg) ? weightKg : null,
      gender,
      medicines
    };
    
    const registerUser = async () => {
      try {
        const response = await smtiaApi.auth.registerOnboarding(registerPayload);
        
        // Check if email confirmation is required
        if (response?.requiresEmailConfirmation) {
          setIsProcessing(false);
          setRegistrationMessage(response?.message || `${email} adresine onay e-postası gönderildi. Lütfen e-postanızı kontrol edin ve hesabınızı aktifleştirin.`);
          setShowSuccessScreen(true);
          
          // Don't redirect - show email confirmation message
          // User will be redirected to login after email confirmation
          return;
        }
        
        // If token is returned (shouldn't happen for new registrations, but handle it)
        if (response?.token) {
          setToken(response.token);
          setRegistrationMessage('Kayıt başarılı! Dashboard\'a yönlendiriliyorsunuz…');
          setIsProcessing(false);
          setShowSuccessScreen(true);

          setTimeout(() => {
            setShowSuccessScreen(false);
            window.location.href = '/dashboard';
          }, 1200);
        } else {
          // Fallback: show success message
          setIsProcessing(false);
          setRegistrationMessage('Kayıt başarılı! E-posta adresinize onay linki gönderildi.');
          setShowSuccessScreen(true);
        }
      } catch (error) {
        console.error('API call failed:', error);
        setIsProcessing(false);
        alert(error?.message || 'Kayıt işlemi başarısız oldu. Lütfen tekrar deneyin.');
      }
    };
    
    registerUser();
  };

  const getMedicineIcon = (type) => {
    const icons = {
      capsule: capsuleType1,
      pill: capsuleType2,
      bottle: capsuleType3,
      syringe: capsuleType4
    };
    return icons[type] || capsuleType1;
  };

  const handleRemoveMedicine = (index) => {
    if (isProcessing || isTransitioning) return;
    
    setIsProcessing(true);
    setTimeout(() => {
      setAddedMedicines(prev => prev.filter((_, i) => i !== index));
      setIsProcessing(false);
    }, 100);
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

  const handleKeyDown = (e) => {
    if (e.key === 'Escape') {
      handleBack();
    } else if (e.key === 'Enter') {
      handleAddCapsule();
    }
  };

  useEffect(() => {
    const container = document.querySelector('.add-capsule-container');
    if (container) {
      container.focus();
    }
  }, []);

  if (showSuccessScreen) {
    return (
      <div className="success-screen-container">
        <div className="success-screen-header">
          <div className="success-screen-logo-section">
            <img src={logo} alt="Logo" className="success-screen-logo" />
            <span className="success-screen-group-text">
              <span className="success-screen-by-text">by</span> <span className="success-screen-group-name">Group X</span>
            </span>
          </div>
        </div>

        <div className="success-screen-content">
          <div className="success-screen-icon">
            <div className="success-checkmark">
              <svg width="60" height="60" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"/>
                <polyline points="22,4 12,14.01 9,11.01"/>
              </svg>
            </div>
          </div>

          <div className="success-screen-title">
            <h1>Kayıt Başarılı!</h1>
            <p className="success-screen-subtitle">
              {registrationMessage || 'E-posta adresinize onay linki gönderildi. Lütfen e-postanızı kontrol edin.'}
            </p>
          </div>
          
          <div style={{ 
            marginTop: 24, 
            padding: '20px', 
            backgroundColor: '#f8f6f3', 
            borderRadius: '12px',
            textAlign: 'left',
            maxWidth: '500px',
            margin: '24px auto 0'
          }}>
            <p style={{ marginBottom: '12px', color: '#5a5a5a', fontSize: '14px' }}>
              <strong style={{ color: '#4a4a4a' }}>📧 E-postanızı kontrol edin</strong>
            </p>
            <p style={{ marginBottom: '8px', color: '#5a5a5a', fontSize: '14px', lineHeight: '1.6' }}>
              Gelen kutunuzda SMTIA'dan bir e-posta bulacaksınız. E-postadaki linke tıklayarak hesabınızı aktifleştirin.
            </p>
            <p style={{ marginTop: '12px', color: '#7a7a7a', fontSize: '13px', fontStyle: 'italic' }}>
              💡 İpucu: E-posta gelmediyse spam klasörünüzü kontrol edin.
            </p>
          </div>
          
          <button
            onClick={() => window.location.href = '/'}
            style={{
              marginTop: '32px',
              padding: '14px 32px',
              backgroundColor: '#a8c8a0',
              color: 'white',
              border: 'none',
              borderRadius: '12px',
              fontSize: '16px',
              fontWeight: '500',
              cursor: 'pointer',
              transition: 'all 0.3s ease',
              boxShadow: '0 4px 12px rgba(141, 184, 125, 0.3)'
            }}
            onMouseEnter={(e) => {
              e.target.style.transform = 'translateY(-2px)';
              e.target.style.boxShadow = '0 6px 16px rgba(141, 184, 125, 0.4)';
            }}
            onMouseLeave={(e) => {
              e.target.style.transform = 'translateY(0)';
              e.target.style.boxShadow = '0 4px 12px rgba(141, 184, 125, 0.3)';
            }}
          >
            Ana Sayfaya Dön
          </button>
        </div>
      </div>
    );
  }

  return (
    <div 
      className="add-capsule-container"
      onKeyDown={handleKeyDown}
      tabIndex={0}
    >
      <div className="add-capsule-header">
        <div className="add-capsule-logo-section">
          <img src={logo} alt="Logo" className="add-capsule-logo" />
          <span className="add-capsule-group-text">
            <span className="add-capsule-by-text">by</span> <span className="add-capsule-group-name">Group X</span>
          </span>
        </div>
        <div className="add-capsule-header-right">
          <div 
            className="add-capsule-back-button" 
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
          <div className="add-capsule-progress-indicator">
            {currentStep}/{totalSteps}
          </div>
        </div>
      </div>

      <div className="add-capsule-content">
        <div className="add-capsule-title">
          <h1>İlaç Ekle</h1>
          <p className="add-capsule-subtitle">
            Yeni bir ilaç ekleyerek takip etmeye başlayabilirsiniz.
          </p>
        </div>

        <div className="add-capsule-input-container">
          <div className="add-capsule-options">
            {addedMedicines.map((medicine, index) => (
              <div key={index} className="add-capsule-medicine-item">
                <button 
                  className="medicine-remove-button"
                  onClick={() => handleRemoveMedicine(index)}
                  title="İlacı kaldır"
                >
                  <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                    <line x1="18" y1="6" x2="6" y2="18"/>
                    <line x1="6" y1="6" x2="18" y2="18"/>
                  </svg>
                </button>
                <img src={getMedicineIcon(medicine.type)} alt={medicine.name} className="medicine-icon" />
                <div className="medicine-name">{medicine.name}</div>
              </div>
            ))}
            
            <div
              className="add-capsule-option"
              onClick={handleAddCapsule}
            >
              <div className="add-capsule-icon">
                <svg width="60" height="60" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                  <line x1="12" y1="5" x2="12" y2="19"/>
                  <line x1="5" y1="12" x2="19" y2="12"/>
                </svg>
              </div>
            </div>
          </div>
        </div>
      </div>

      {showKVKKMessage && (
        <div className="add-capsule-kvkk-message">
          <div className="kvkk-content">
            <span className="kvkk-text">Gizliliğinizde önem veriyoruz. Daha fazla bilgi için Veri ve Çerez Politikasını ziyaret edebilirsiniz.</span>
            <button className="kvkk-button" onClick={onKVKKClick}>KVKK</button>
          </div>
          <button className="kvkk-close" onClick={onKVKKClose}>
            <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
              <line x1="18" y1="6" x2="6" y2="18"/>
              <line x1="6" y1="6" x2="18" y2="18"/>
            </svg>
          </button>
        </div>
      )}

      <AddMedicineModal
        isOpen={showModal && !showScheduleModal && !isTransitioning && !isProcessing}
        onClose={handleModalClose}
        onNext={handleModalNext}
        initialData={currentMedicineData}
      />

      <AddMedicineScheduleModal
        isOpen={showScheduleModal && !showModal && !isTransitioning && !isProcessing}
        onClose={handleScheduleModalClose}
        onBack={handleScheduleModalBack}
        onAddMedicine={handleAddMedicine}
        medicineData={currentMedicineData}
      />

      {addedMedicines.length > 0 && !showSuccessScreen && (
        <div className="complete-registration-container">
          <button 
            className="complete-registration-button"
            onClick={handleCompleteRegistration}
            disabled={isProcessing || isTransitioning}
          >
            <span className="complete-registration-text">Kayıtı Tamamla</span>
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
              <path d="M9 18l6-6-6-6"/>
            </svg>
          </button>
        </div>
      )}

    </div>
  );
};

export default AddCapsulePage;
