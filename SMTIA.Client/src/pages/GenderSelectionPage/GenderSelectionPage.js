import React, { useState, useEffect } from 'react';
import './GenderSelectionPage.css';
import logo from '../../assets/logos/logo.png';
import AddCapsulePage from '../AddCapsulePage/AddCapsulePage';

const GenderSelectionPage = ({ onGenderSelected, onBack, currentStep = 5, totalSteps = 6, showKVKKMessage = false, onKVKKClose, onKVKKClick, userRegistrationData = {} }) => {
  const [selectedGender, setSelectedGender] = useState(null);
  const [isTransitioning, setIsTransitioning] = useState(false);
  const [showBackTooltip, setShowBackTooltip] = useState(false);
  const [showAddCapsule, setShowAddCapsule] = useState(false);

  const genderOptions = [
    { id: 'male', label: 'Erkek', icon: '♂' },
    { id: 'female', label: 'Kadın', icon: '♀' }
  ];

  const handleGenderSelect = (gender) => {
    if (gender !== selectedGender) {
      setSelectedGender(gender);
    }
  };

  const handleNext = () => {
    if (selectedGender) {
      setIsTransitioning(true);
      setTimeout(() => {
        setShowAddCapsule(true);
      }, 300);
    }
  };

  const handleBack = () => {
    setIsTransitioning(true);
    setTimeout(() => {
      onBack();
    }, 200);
  };

  const handleKeyDown = (e) => {
    if (e.key === 'Escape') {
      handleBack();
    } else if (e.key === 'Enter') {
      handleNext();
    } else if (e.key === 'ArrowLeft') {
      e.preventDefault();
      if (selectedGender === 'female') {
        setSelectedGender('male');
      }
    } else if (e.key === 'ArrowRight') {
      e.preventDefault();
      if (selectedGender === 'male') {
        setSelectedGender('female');
      }
    }
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

  const handleBackFromAddCapsule = () => {
    setIsTransitioning(true);
    setTimeout(() => {
      setShowAddCapsule(false);
      setIsTransitioning(false);
    }, 200);
  };

  const handleAddCapsuleComplete = () => {
    onGenderSelected(selectedGender);
  };

  useEffect(() => {
    const container = document.querySelector('.gender-selection-container');
    if (container) {
      container.focus();
    }
  }, []);

  if (showAddCapsule) {
    return <AddCapsulePage 
      onBack={handleBackFromAddCapsule} 
      currentStep={6} 
      totalSteps={6}
      showKVKKMessage={showKVKKMessage}
      onKVKKClose={onKVKKClose}
      onKVKKClick={onKVKKClick}
      userRegistrationData={{...userRegistrationData, gender: selectedGender}}
    />;
  }

  return (
    <div 
      className="gender-selection-container"
      onKeyDown={handleKeyDown}
      tabIndex={0}
    >
      <div className="gender-selection-header">
        <div className="gender-selection-logo-section">
          <img src={logo} alt="Logo" className="gender-selection-logo" />
          <span className="gender-selection-group-text">
            <span className="gender-selection-by-text">by</span> <span className="gender-selection-group-name">Group X</span>
          </span>
        </div>
        <div className="gender-selection-header-right">
          <div 
            className="gender-selection-back-button" 
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
          <div className="gender-selection-progress-indicator">
            {currentStep}/{totalSteps}
          </div>
        </div>
      </div>

      <div className="gender-selection-content">
        <div className={`gender-selection-title ${isTransitioning ? 'fade-out' : 'fade-in'}`}>
          <h1>Cinsiyetinizi Seçiniz</h1>
          <p className="gender-selection-subtitle">
            Cinsiyet bilginiz, ilaç dozajlarının vücut yapınıza göre hesaplanmasına yardımcı olur.
          </p>
        </div>

        <div className={`gender-selection-input-container ${isTransitioning ? 'fade-out' : 'fade-in'}`}>
          <div className="gender-selection-options">
            {genderOptions.map((option) => (
              <div
                key={option.id}
                className={`gender-selection-option ${selectedGender === option.id ? 'selected' : ''}`}
                data-gender={option.id}
                onClick={() => handleGenderSelect(option.id)}
              >
                <div className="gender-icon">{option.icon}</div>
                <div className="gender-label">{option.label}</div>
              </div>
            ))}
          </div>
        </div>
      </div>

      <div className={`gender-selection-next-page-button ${isTransitioning ? 'fade-out' : 'fade-in'} ${!selectedGender ? 'disabled' : ''}`} onClick={selectedGender ? handleNext : undefined}>
        <span>Sonraki Sayfa</span>
        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
          <path d="M5 12h14"/>
          <path d="M12 5l7 7-7 7"/>
        </svg>
      </div>

      {showKVKKMessage && (
        <div className="gender-selection-kvkk-message">
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
    </div>
  );
};

export default GenderSelectionPage;
