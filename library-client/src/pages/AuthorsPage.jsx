import React, { useState, useCallback } from 'react';
import { useAuthors } from '../hooks/useData';
import { authorsApi } from '../services/api';
import Modal from '../components/Modal';
import { Loading, EmptyState, ErrorBanner } from '../components/StatusComponents';
import { Plus, Pencil, Trash2, UserCircle } from 'lucide-react';
import toast from 'react-hot-toast';

const emptyForm = { firstName: '', lastName: '', bio: '', dateOfBirth: '' };

function AuthorsPage() {
  const { data: authors, loading, error, refresh } = useAuthors();
  const [showModal, setShowModal] = useState(false);
  const [editing, setEditing] = useState(null);
  const [form, setForm] = useState(emptyForm);
  const [saving, setSaving] = useState(false);

  const openCreate = useCallback(() => {
    setEditing(null);
    setForm(emptyForm);
    setShowModal(true);
  }, []);

  const openEdit = useCallback((author) => {
    setEditing(author);
    setForm({
      firstName: author.firstName,
      lastName: author.lastName,
      bio: author.bio || '',
      dateOfBirth: author.dateOfBirth?.split('T')[0] || '',
    });
    setShowModal(true);
  }, []);

  const handleSubmit = useCallback(async (e) => {
    e.preventDefault();
    setSaving(true);
    try {
      if (editing) {
        await authorsApi.update(editing.id, form);
        toast.success('Author updated');
      } else {
        await authorsApi.create(form);
        toast.success('Author created');
      }
      setShowModal(false);
      refresh();
    } catch (err) {
      toast.error(err.response?.data?.message || 'Failed to save');
    } finally {
      setSaving(false);
    }
  }, [form, editing, refresh]);

  const handleDelete = useCallback(async (author) => {
    if (!window.confirm(`Delete ${author.firstName} ${author.lastName}?`)) return;
    try {
      await authorsApi.delete(author.id);
      toast.success('Author deleted');
      refresh();
    } catch (err) {
      toast.error(err.response?.data?.message || 'Failed to delete');
    }
  }, [refresh]);

  const handleChange = useCallback((e) => {
    const { name, value } = e.target;
    setForm(prev => ({ ...prev, [name]: value }));
  }, []);

  if (loading) return <Loading />;

  return (
    <div>
      <div className="page-header page-header-actions">
        <div>
          <h2>Authors</h2>
          <p>Manage book authors</p>
        </div>
        <button className="btn btn-primary" onClick={openCreate}>
          <Plus size={18} /> Add Author
        </button>
      </div>

      <ErrorBanner message={error} />

      <div className="card">
        {authors.length > 0 ? (
          <table className="data-table">
            <thead>
              <tr>
                <th>Name</th>
                <th>Date of Birth</th>
                <th>Books</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {authors.map((author) => (
                <tr key={author.id}>
                  <td style={{ fontWeight: 500 }}>{author.firstName} {author.lastName}</td>
                  <td>{new Date(author.dateOfBirth).toLocaleDateString()}</td>
                  <td><span className="badge badge-accent">{author.bookCount}</span></td>
                  <td>
                    <div className="action-btns">
                      <button className="btn btn-ghost btn-sm" onClick={() => openEdit(author)}><Pencil size={14} /></button>
                      <button className="btn btn-ghost btn-sm" onClick={() => handleDelete(author)} style={{ color: 'var(--color-danger)' }}><Trash2 size={14} /></button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        ) : (
          <EmptyState icon={UserCircle} title="No authors yet" message="Add your first author to get started." />
        )}
      </div>

      <Modal
        isOpen={showModal}
        onClose={() => setShowModal(false)}
        title={editing ? 'Edit Author' : 'Add New Author'}
        footer={
          <>
            <button className="btn btn-ghost" onClick={() => setShowModal(false)}>Cancel</button>
            <button className="btn btn-primary" onClick={handleSubmit} disabled={saving}>
              {saving ? 'Saving...' : (editing ? 'Update' : 'Create')}
            </button>
          </>
        }
      >
        <form onSubmit={handleSubmit}>
          <div className="form-row">
            <div className="form-group">
              <label className="form-label">First Name *</label>
              <input className="form-input" name="firstName" value={form.firstName} onChange={handleChange} required />
            </div>
            <div className="form-group">
              <label className="form-label">Last Name *</label>
              <input className="form-input" name="lastName" value={form.lastName} onChange={handleChange} required />
            </div>
          </div>
          <div className="form-group">
            <label className="form-label">Date of Birth</label>
            <input className="form-input" type="date" name="dateOfBirth" value={form.dateOfBirth} onChange={handleChange} />
          </div>
          <div className="form-group">
            <label className="form-label">Bio</label>
            <textarea className="form-textarea" name="bio" value={form.bio} onChange={handleChange} />
          </div>
        </form>
      </Modal>
    </div>
  );
}

export default AuthorsPage;
