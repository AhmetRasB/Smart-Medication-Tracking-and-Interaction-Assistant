import React, { useState } from 'react';
import './IntroPage.css';
import logo from '../../assets/logos/logo.png';
import capsule from '../../assets/images/capsule.png';
import intropage from '../../assets/images/intropage-2.png';
import UserRegistrationPage from '../UserRegistrationPage/UserRegistrationPage';
import { smtiaApi } from '../../services/smtiaApi';

const IntroPage = ({ onGetStarted, onLoginSuccess }) => {
  const [loginStep, setLoginStep] = useState('start'); 
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [showRegistration, setShowRegistration] = useState(false);
  const [isTransitioning, setIsTransitioning] = useState(false);
  const [isLoggingIn, setIsLoggingIn] = useState(false);
  const [showLoginSuccess, setShowLoginSuccess] = useState(false);
  const [isLoginSuccessFading, setIsLoginSuccessFading] = useState(false);
  const [emailError, setEmailError] = useState('');
  
  const [forgotPasswordStep, setForgotPasswordStep] = useState(null);
  const [resetEmail, setResetEmail] = useState('');
  const [resetEmailError, setResetEmailError] = useState('');
  const [codeSentMessage, setCodeSentMessage] = useState('');
  const [isSendingCode, setIsSendingCode] = useState(false);

  const handleGetStartedClick = () => {
    setIsTransitioning(true);
    setTimeout(() => {
      setShowRegistration(true);
      setIsTransitioning(false);
    }, 300);
  };

  const handleRegistrationComplete = (userData) => {
    console.log('Registration completed with:', userData);
    setIsTransitioning(true);
    setTimeout(() => {
      setTimeout(() => {
        onGetStarted();
      }, 200);
    }, 300);
  };

  const handleBackToIntro = () => {
    setIsTransitioning(true);
    setTimeout(() => {
      setShowRegistration(false);
      setIsTransitioning(false);
    }, 300);
  };

  const handleLoginClick = () => {
    setLoginStep('email');
  };

  const handleEmailNext = () => {
    if (email.trim()) {
      const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
      if (!emailRegex.test(email.trim())) {
        setEmailError('Geçerli bir e-posta adresi giriniz');
        return;
      }
      
      setEmailError(''); 
      setLoginStep('password');
    }
  };

  const handlePasswordLogin = async () => {
    if (password.trim() && !isLoggingIn) {
      setIsLoggingIn(true);
      
      try {
        const loginData = await smtiaApi.auth.login(email, password);
        // TS.Result unwrap -> { token, refreshToken, refreshTokenExpires }
        if (!loginData || !loginData.token) {
          throw new Error('Giriş başarısız: token alınamadı');
        }

        setIsLoggingIn(false);
        setShowLoginSuccess(true);

        setTimeout(() => {
          setIsLoginSuccessFading(true);
          setTimeout(() => {
            if (onLoginSuccess) onLoginSuccess(loginData.token);
            else onGetStarted();
          }, 300);
        }, 900);
      } catch (error) {
        console.error('Login API call failed:', error);
        setIsLoggingIn(false);
        
        // Check for specific error messages
        const errorMessage = error?.message || 'Giriş başarısız oldu. Bilgilerinizi kontrol edin.';
        
        // Email confirmation required message
        if (errorMessage.includes('onaylanmamış') || errorMessage.includes('onay linki')) {
          alert(errorMessage);
        }
        // Rate limiting message
        else if (error?.status === 429 || errorMessage.includes('çok fazla') || errorMessage.includes('rate limit')) {
          alert('Çok fazla deneme yaptınız. Lütfen 1 saat sonra tekrar deneyin.');
        }
        // Lockout message
        else if (errorMessage.includes('bloke') || errorMessage.includes('kilitlendi')) {
          alert(errorMessage);
        }
        // Generic error
        else {
          alert(errorMessage);
        }
      }
    }
  };

  const handleEmailKeyPress = (e) => {
    if (e.key === 'Enter' && email.trim()) {
      handleEmailNext();
    }
  };

  const handlePasswordKeyPress = (e) => {
    if (e.key === 'Enter' && password.trim()) {
      handlePasswordLogin();
    }
  };

  const handleKeyDown = (e) => {
    if (e.key === 'Escape') {
      if (loginStep === 'email' || loginStep === 'password') {
        handleBackToStart();
      }
    }
  };

  const handleBackToStart = () => {
    setLoginStep('start');
    setEmail('');
    setPassword('');
    setIsLoggingIn(false);
    setShowLoginSuccess(false);
    setIsLoginSuccessFading(false);
    setEmailError('');
    setForgotPasswordStep(null);
    setResetEmail('');
    setIsSendingCode(false);
    setResetEmailError('');
    setCodeSentMessage('');
  };

  const handleForgotPasswordClick = () => {
    setForgotPasswordStep('email');
    setResetEmail(email); 
  };

  const handleResetEmailSubmit = async () => {
    if (!resetEmail.trim()) {
      setResetEmailError('E-posta adresi gereklidir');
      return;
    }
    
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(resetEmail.trim())) {
      setResetEmailError('Geçerli bir e-posta adresi giriniz');
      return;
    }
    
    setResetEmailError('');
    setIsSendingCode(true);
    
    try {
      await smtiaApi.auth.forgotPassword(resetEmail.trim());
      setIsSendingCode(false);
      setCodeSentMessage(`Şifre sıfırlama linki ${resetEmail} adresine gönderildi. Lütfen e-postanızı kontrol edin.`);
      setForgotPasswordStep('sent');
    } catch (error) {
      console.error('Reset code send failed:', error);
      setIsSendingCode(false);
      
      // Check for rate limiting error
      if (error?.status === 429 || error?.message?.includes('çok fazla') || error?.message?.includes('rate limit')) {
        setResetEmailError('Çok fazla deneme yaptınız. Lütfen 1 saat sonra tekrar deneyin.');
      } else {
        setResetEmailError(error?.message || 'İstek başarısız oldu. Lütfen tekrar deneyin.');
      }
    }
  };

  if (showRegistration) {
    return (
      <div className="intro-container fade-in">
        <UserRegistrationPage 
          onRegistrationComplete={handleRegistrationComplete}
          onBackToIntro={handleBackToIntro}
        />
      </div>
    );
  }

  if (showLoginSuccess) {
    return (
      <div className={`intro-container fade-in ${isLoginSuccessFading ? 'fade-out' : ''}`}>
        <div className="intro-header">
          <img src={logo} alt="Logo" className="intro-logo" />
          <span className="intro-group-text">
            <span className="intro-by-text">by</span> <span className="intro-group-name">Group X</span>
          </span>
        </div>

        <div className="intro-content">
          <div className="login-success-icon">
            <div className="success-checkmark">
              <svg width="60" height="60" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"/>
                <polyline points="22,4 12,14.01 9,11.01"/>
              </svg>
            </div>
          </div>
          
          <div className="login-success-title">
            <h1>Başarıyla Giriş Yaptınız!</h1>
            <p className="login-success-subtitle">
              Hoş geldiniz! İlaç takip uygulamanızı kullanmaya başlayabilirsiniz.
            </p>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className={`intro-container ${isTransitioning ? 'fade-out' : 'fade-in'}`}>

      <div className="intro-header">
        <img src={logo} alt="Logo" className="intro-logo" />
        <span className="intro-group-text">
          <span className="intro-by-text">by</span> <span className="intro-group-name">Group X</span>
        </span>
      </div>

      <div className="intro-content">
        {!forgotPasswordStep && (
          <img src={intropage} alt="Intro page" className="intro-main-image" />
        )}
        
        {loginStep === 'start' && !forgotPasswordStep && (
          <>
        
            <div className="intro-button" onClick={handleGetStartedClick}>
              <span className="intro-button-text">Başla</span>
            </div>
            
            <div className="intro-mobile-login-button" onClick={handleLoginClick}>
              <span className="intro-mobile-login-text">Giriş Yap</span>
            </div>
            
            <div className="intro-login-text">
              <span className="intro-login-question">Zaten hesabın var mı?</span>
              <span className="intro-login-link" onClick={handleLoginClick}>Giriş yap</span>
            </div>


          </>
        )}

        {loginStep === 'email' && !forgotPasswordStep && (
          <div className="intro-input-container">
            <div className={`intro-input-wrapper ${emailError ? 'error' : ''}`}>
              <input
                type="email"
                placeholder="E-posta adresinizi girin"
                value={email}
                onChange={(e) => {
                  setEmail(e.target.value);
                  setEmailError(''); 
                }}
                onKeyPress={handleEmailKeyPress}
                onKeyDown={handleKeyDown}
                className={`intro-input ${emailError ? 'error' : ''}`}
                autoFocus
              />
              <button 
                className="intro-input-button" 
                onClick={handleEmailNext}
                disabled={!email.trim()}
              >
                İleri
              </button>
            </div>
            
            <div className={`intro-input-wrapper-mobile ${emailError ? 'error' : ''}`}>
              <input
                type="email"
                placeholder="E-posta adresinizi girin"
                value={email}
                onChange={(e) => {
                  setEmail(e.target.value);
                  setEmailError(''); 
                }}
                onKeyPress={handleEmailKeyPress}
                onKeyDown={handleKeyDown}
                className={`intro-input-mobile ${emailError ? 'error' : ''}`}
                autoFocus
              />
              <button 
                className="intro-input-button-mobile" 
                onClick={handleEmailNext}
                disabled={!email.trim()}
              >
                İleri
              </button>
            </div>
            
            {emailError && (
              <div className="intro-error-message">
                {emailError}
              </div>
            )}
            
            <div className="intro-links-container">
              <div className="intro-back-link" onClick={handleBackToStart}>
                ← Geri dön
              </div>
              <div className="intro-forgot-password" onClick={handleForgotPasswordClick}>
                Şifremi Unuttum
              </div>
            </div>
            
            <div className="intro-links-container-mobile">
              <div className="intro-back-link-mobile" onClick={handleBackToStart}>
                ← Geri dön
              </div>
              <div className="intro-forgot-password-mobile" onClick={handleForgotPasswordClick}>
                Şifremi Unuttum
              </div>
            </div>
          </div>
        )}

        {forgotPasswordStep === 'email' && (
          <div className="intro-input-container">
            <div className="intro-reset-title">
              <h2>Şifremi Unuttum</h2>
              <p className="intro-reset-subtitle">E-posta adresinize doğrulama kodu göndereceğiz</p>
            </div>
            
            <div className={`intro-input-wrapper ${resetEmailError ? 'error' : ''}`}>
              <input
                type="email"
                placeholder="E-posta adresinizi girin"
                value={resetEmail}
                onChange={(e) => {
                  setResetEmail(e.target.value);
                  setResetEmailError('');
                }}
                onKeyPress={(e) => {
                  if (e.key === 'Enter' && resetEmail.trim()) {
                    handleResetEmailSubmit();
                  }
                }}
                className={`intro-input ${resetEmailError ? 'error' : ''}`}
                autoFocus
              />
              <button 
                className="intro-input-button" 
                onClick={handleResetEmailSubmit}
                disabled={!resetEmail.trim() || isSendingCode}
              >
                {isSendingCode ? (
                  <div className="spinner"></div>
                ) : (
                  'Kod Gönder'
                )}
              </button>
            </div>
            
            <div className={`intro-input-wrapper-mobile ${resetEmailError ? 'error' : ''}`}>
              <input
                type="email"
                placeholder="E-posta adresinizi girin"
                value={resetEmail}
                onChange={(e) => {
                  setResetEmail(e.target.value);
                  setResetEmailError('');
                }}
                onKeyPress={(e) => {
                  if (e.key === 'Enter' && resetEmail.trim()) {
                    handleResetEmailSubmit();
                  }
                }}
                className={`intro-input-mobile ${resetEmailError ? 'error' : ''}`}
                autoFocus
              />
              <button 
                className="intro-input-button-mobile" 
                onClick={handleResetEmailSubmit}
                disabled={!resetEmail.trim() || isSendingCode}
              >
                {isSendingCode ? (
                  <div className="spinner"></div>
                ) : (
                  'Kod Gönder'
                )}
              </button>
            </div>
            
            {resetEmailError && (
              <div className="intro-error-message">
                {resetEmailError}
              </div>
            )}
            
            <div className="intro-back-link" onClick={() => setForgotPasswordStep(null)}>
              ← Geri dön
            </div>
            
            <div className="intro-back-link-mobile" onClick={() => setForgotPasswordStep(null)}>
              ← Geri dön
            </div>
          </div>
        )}

        {forgotPasswordStep === 'sent' && (
          <div className="intro-input-container">
            <div className="intro-reset-title">
              <h2>Link Gönderildi</h2>
              <p className="intro-reset-subtitle">{codeSentMessage || `Şifre sıfırlama linki ${resetEmail} adresine gönderildi`}</p>
            </div>

            <div className="intro-links-container" style={{ justifyContent: 'center' }}>
              <div className="intro-back-link" onClick={handleBackToStart}>
                ← Giriş ekranına dön
              </div>
            </div>
          </div>
        )}

        {loginStep === 'password' && forgotPasswordStep === null && (
          <div className="intro-input-container">
            <div className="intro-input-wrapper">
              <input
                type="password"
                placeholder="Şifrenizi girin"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                onKeyPress={handlePasswordKeyPress}
                onKeyDown={handleKeyDown}
                className="intro-input"
                autoFocus
              />
              <button 
                className="intro-input-button" 
                onClick={handlePasswordLogin}
                disabled={!password.trim() || isLoggingIn}
              >
                {isLoggingIn ? (
                  <div className="spinner"></div>
                ) : (
                  'Giriş Yap'
                )}
              </button>
            </div>
            
            <div className="intro-input-wrapper-mobile">
              <input
                type="password"
                placeholder="Şifrenizi girin"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                onKeyPress={handlePasswordKeyPress}
                onKeyDown={handleKeyDown}
                className="intro-input-mobile"
                autoFocus
              />
              <button 
                className="intro-input-button-mobile" 
                onClick={handlePasswordLogin}
                disabled={!password.trim() || isLoggingIn}
              >
                {isLoggingIn ? (
                  <div className="spinner"></div>
                ) : (
                  'Giriş Yap'
                )}
              </button>
            </div>
            
            <div className="intro-back-link" onClick={handleBackToStart}>
              ← Geri dön
            </div>
            
            <div className="intro-back-link-mobile" onClick={handleBackToStart}>
              ← Geri dön
            </div>
          </div>
        )}
        
      </div>


    </div>
  );
};

export default IntroPage;
