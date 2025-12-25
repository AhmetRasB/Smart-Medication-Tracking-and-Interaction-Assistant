import React, { useEffect, useMemo, useState } from 'react';
import './ModeratorPanel.css';
import { smtiaApi } from '../../services/smtiaApi';

const emptyForm = {
  name: '',
  description: '',
  dosageForm: '',
  activeIngredient: '',
  manufacturer: '',
  barcode: ''
};

const ModeratorPanel = ({ onClose }) => {
  const [medicines, setMedicines] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [search, setSearch] = useState('');
  const [form, setForm] = useState(emptyForm);
  const [saving, setSaving] = useState(false);

  const filtered = useMemo(() => {
    const q = search.trim().toLowerCase();
    if (!q) return medicines;
    return medicines.filter(m =>
      (m.name || '').toLowerCase().includes(q) ||
      (m.activeIngredient || '').toLowerCase().includes(q) ||
      (m.manufacturer || '').toLowerCase().includes(q)
    );
  }, [medicines, search]);

  useEffect(() => {
    let mounted = true;
    const load = async () => {
      setLoading(true);
      setError('');
      try {
        const res = await smtiaApi.medicines.getAll();
        if (!mounted) return;
        // API returns TS.Result -> unwrap gives list
        setMedicines(Array.isArray(res) ? res : (res?.medicines || []));
      } catch (e) {
        if (!mounted) return;
        setError(e?.message || 'İlaçlar yüklenemedi');
      } finally {
        if (mounted) setLoading(false);
      }
    };
    load();
    return () => { mounted = false; };
  }, []);

  const handleCreate = async () => {
    if (!form.name.trim()) {
      alert('İlaç adı zorunludur');
      return;
    }
    setSaving(true);
    try {
      await smtiaApi.medicines.create({
        name: form.name.trim(),
        description: form.description || null,
        dosageForm: form.dosageForm || null,
        activeIngredient: form.activeIngredient || null,
        manufacturer: form.manufacturer || null,
        barcode: form.barcode || null
      });
      // Reload list (simple)
      const res = await smtiaApi.medicines.getAll();
      setMedicines(Array.isArray(res) ? res : (res?.medicines || []));
      setForm(emptyForm);
    } catch (e) {
      alert(e?.message || 'İlaç eklenemedi');
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async (id) => {
    if (!window.confirm('Bu ilacı silmek istiyor musunuz?')) return;
    try {
      await smtiaApi.medicines.delete(id);
      setMedicines(prev => prev.filter(m => m.id !== id));
    } catch (e) {
      alert(e?.message || 'Silinemedi');
    }
  };

  return (
    <div className="mod-panel-overlay" onClick={onClose}>
      <div className="mod-panel" onClick={(e) => e.stopPropagation()}>
        <div className="mod-header">
          <div>
            <h2>Moderator Panel</h2>
            <p>İlaç veritabanını yönetin</p>
          </div>
          <button className="mod-close" onClick={onClose}>✕</button>
        </div>

        <div className="mod-grid">
          <div className="mod-card">
            <h3>Yeni ilaç ekle</h3>
            <div className="mod-form">
              <input placeholder="İlaç adı *" value={form.name} onChange={e => setForm({ ...form, name: e.target.value })} />
              <input placeholder="Etken madde" value={form.activeIngredient} onChange={e => setForm({ ...form, activeIngredient: e.target.value })} />
              <input placeholder="Dozaj formu" value={form.dosageForm} onChange={e => setForm({ ...form, dosageForm: e.target.value })} />
              <input placeholder="Üretici" value={form.manufacturer} onChange={e => setForm({ ...form, manufacturer: e.target.value })} />
              <input placeholder="Barkod" value={form.barcode} onChange={e => setForm({ ...form, barcode: e.target.value })} />
              <textarea placeholder="Açıklama" value={form.description} onChange={e => setForm({ ...form, description: e.target.value })} />
              <button className="mod-primary" onClick={handleCreate} disabled={saving}>
                {saving ? 'Kaydediliyor…' : 'Ekle'}
              </button>
            </div>
          </div>

          <div className="mod-card">
            <h3>İlaçlar</h3>
            <input
              className="mod-search"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              placeholder="Ara (ad / etken madde / üretici)"
            />

            {loading && <div className="mod-state">Yükleniyor…</div>}
            {!loading && error && <div className="mod-error">{error}</div>}

            {!loading && !error && (
              <div className="mod-list">
                {filtered.slice(0, 100).map(m => (
                  <div key={m.id} className="mod-item">
                    <div className="mod-item-main">
                      <strong>{m.name}</strong>
                      <div className="mod-item-sub">
                        {m.activeIngredient ? `Etken: ${m.activeIngredient}` : '—'}
                      </div>
                    </div>
                    <button className="mod-danger" onClick={() => handleDelete(m.id)}>Sil</button>
                  </div>
                ))}
                {filtered.length > 100 && (
                  <div className="mod-state">İlk 100 kayıt gösteriliyor (filtreyi daraltın).</div>
                )}
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

export default ModeratorPanel;


