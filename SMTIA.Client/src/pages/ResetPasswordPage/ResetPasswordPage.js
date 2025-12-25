import React, { useState, useEffect } from 'react';
import { setToken as saveTokenToStorage, getUserFromToken } from '../../services/auth';
import { smtiaApi } from '../../services/smtiaApi';
import './ResetPasswordPage.css';

function ResetPasswordPage() {
  const [email, setEmail] = useState('');
  const [token, setToken] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [error, setError] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [success, setSuccess] = useState(false);

  useEffect(() => {
    const urlParams = new URLSearchParams(window.location.search);
    const emailParam = urlParams.get('email');
    const tokenParam = urlParams.get('token');
    
    if (emailParam && tokenParam) {
      setEmail(emailParam);
      setToken(tokenParam);
    } else {
      setError('Geçersiz şifre sıfırlama linki.');
    }
  }, []);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');

    if (newPassword !== confirmPassword) {
      setError('Şifreler eşleşmiyor. Lütfen tekrar kontrol edin.');
      return;
    }

    if (newPassword.length < 6) {
      setError('Şifre en az 6 karakter olmalıdır.');
      return;
    }

    setIsSubmitting(true);

    try {
      const response = await smtiaApi.auth.resetPassword({
        email,
        token,
        newPassword
      });

      if (response?.success && response?.token) {
        // Token'ı localStorage'a kaydet
        saveTokenToStorage(response.token);
        const user = getUserFromToken(response.token);
        setSuccess(true);
        
        // 2 saniye sonra dashboard'a yönlendir
        setTimeout(() => {
          window.location.href = '/';
        }, 2000);
      } else {
        setError(response?.message || 'Şifre sıfırlama başarısız oldu. Lütfen tekrar deneyin.');
      }
    } catch (err) {
      setError(err?.message || 'Şifre sıfırlama başarısız oldu. Lütfen tekrar deneyin.');
    } finally {
      setIsSubmitting(false);
    }
  };

  if (success) {
    return (
      <div className="reset-password-page">
        <div className="container">
          <div className="success-icon">✓</div>
          <h1>Şifre Başarıyla Sıfırlandı!</h1>
          <p className="subtitle">
            <span className="email">{email}</span> adresiniz için şifreniz başarıyla sıfırlandı.
          </p>
          <p className="subtitle">
            Otomatik olarak giriş yapılıyor...
          </p>
          <p className="loading-text">Dashboard'a yönlendiriliyorsunuz...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="reset-password-page">
      <div className="container">
        <div className="icon">🔒</div>
        <h1>Şifre Sıfırlama</h1>
        <p className="subtitle">
          Yeni şifrenizi belirleyin. Şifreniz en az 6 karakter olmalıdır.
        </p>
        
        {error && (
          <div className="error-message">{error}</div>
        )}

        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label htmlFor="newPassword">Yeni Şifre</label>
            <input
              type="password"
              id="newPassword"
              value={newPassword}
              onChange={(e) => setNewPassword(e.target.value)}
              required
              minLength={6}
              placeholder="Yeni şifrenizi girin"
            />
          </div>
          
          <div className="form-group">
            <label htmlFor="confirmPassword">Şifre Tekrar</label>
            <input
              type="password"
              id="confirmPassword"
              value={confirmPassword}
              onChange={(e) => setConfirmPassword(e.target.value)}
              required
              minLength={6}
              placeholder="Şifrenizi tekrar girin"
            />
          </div>
          
          <button type="submit" className="button" disabled={isSubmitting}>
            {isSubmitting ? 'Sıfırlanıyor...' : 'Şifremi Sıfırla'}
          </button>
        </form>
      </div>
    </div>
  );
}

export default ResetPasswordPage;

