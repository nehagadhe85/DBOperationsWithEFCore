import React, { useState, useEffect, useCallback, useMemo } from 'react';
import { useBooks, useAuthors, useCategories, useDebounce } from '../hooks/useData';
import { booksApi } from '../services/api';
import Modal from '../components/Modal';
import Pagination from '../components/Pagination';
import { Loading, EmptyState, ErrorBanner } from '../components/StatusComponents';
import { Plus, Search, Pencil, Trash2, BookOpen } from 'lucide-react';
import toast from 'react-hot-toast';

const emptyForm = {
  title: '', isbn: '', description: '', publishedDate: '',
  totalCopies: 1, copiesAvailable: 0, authorId: '', categoryId: '',
};

function BooksPage() {
  const { data: books, loading, error, page, setPage, totalPages, totalCount, pageSize, refresh, updateParams } = useBooks();
  const { data: authors } = useAuthors();
  const { data: categories } = useCategories();

  const [showModal, setShowModal] = useState(false);
  const [editingBook, setEditingBook] = useState(null);
  const [form, setForm] = useState(emptyForm);
  const [saving, setSaving] = useState(false);
  const [searchTerm, setSearchTerm] = useState('');
  const [categoryFilter, setCategoryFilter] = useState('');
  const [authorFilter, setAuthorFilter] = useState('');

  const debouncedSearch = useDebounce(searchTerm, 400);

  // Update search/filter params when debounced value changes
  useEffect(() => {
    updateParams({
      search: debouncedSearch || undefined,
      categoryId: categoryFilter || undefined,
      authorId: authorFilter || undefined,
    });
  }, [debouncedSearch, categoryFilter, authorFilter, updateParams]);

  const openCreateModal = useCallback(() => {
    setEditingBook(null);
    setForm(emptyForm);
    setShowModal(true);
  }, []);

  const openEditModal = useCallback((book) => {
    setEditingBook(book);
    setForm({
      title: book.title,
      isbn: book.isbn,
      description: book.description || '',
      publishedDate: book.publishedDate?.split('T')[0] || '',
      totalCopies: book.totalCopies,
      copiesAvailable: book.copiesAvailable,
      authorId: book.authorId,
      categoryId: book.categoryId,
    });
    setShowModal(true);
  }, []);

  const handleSubmit = useCallback(async (e) => {
    e.preventDefault();
    setSaving(true);
    try {
      const payload = {
        ...form,
        authorId: parseInt(form.authorId),
        categoryId: parseInt(form.categoryId),
        totalCopies: parseInt(form.totalCopies),
        copiesAvailable: parseInt(form.copiesAvailable || form.totalCopies),
      };

      if (editingBook) {
        await booksApi.update(editingBook.id, payload);
        toast.success('Book updated successfully');
      } else {
        await booksApi.create(payload);
        toast.success('Book created successfully');
      }
      setShowModal(false);
      refresh();
    } catch (err) {
      toast.error(err.response?.data?.message || 'Failed to save book');
    } finally {
      setSaving(false);
    }
  }, [form, editingBook, refresh]);

  const handleDelete = useCallback(async (book) => {
    if (!window.confirm(`Delete "${book.title}"? This action uses soft delete.`)) return;
    try {
      await booksApi.delete(book.id);
      toast.success('Book deleted');
      refresh();
    } catch (err) {
      toast.error(err.response?.data?.message || 'Failed to delete book');
    }
  }, [refresh]);

  const handleChange = useCallback((e) => {
    const { name, value } = e.target;
    setForm(prev => ({ ...prev, [name]: value }));
  }, []);

  if (loading && books.length === 0) return <Loading />;

  return (
    <div>
      <div className="page-header page-header-actions">
        <div>
          <h2>Books</h2>
          <p>Manage your library collection</p>
        </div>
        <button className="btn btn-primary" onClick={openCreateModal}>
          <Plus size={18} /> Add Book
        </button>
      </div>

      <ErrorBanner message={error} />

      <div className="toolbar">
        <div className="search-bar">
          <Search />
          <input
            className="form-input"
            placeholder="Search books, authors..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
          />
        </div>
        <select
          className="form-select filter-select"
          value={categoryFilter}
          onChange={(e) => setCategoryFilter(e.target.value)}
        >
          <option value="">All Categories</option>
          {categories.map(c => (
            <option key={c.id} value={c.id}>{c.name}</option>
          ))}
        </select>
        <select
          className="form-select filter-select"
          value={authorFilter}
          onChange={(e) => setAuthorFilter(e.target.value)}
        >
          <option value="">All Authors</option>
          {authors.map(a => (
            <option key={a.id} value={a.id}>{a.fullName}</option>
          ))}
        </select>
      </div>

      <div className="card">
        {books.length > 0 ? (
          <>
            <table className="data-table">
              <thead>
                <tr>
                  <th>Title</th>
                  <th>ISBN</th>
                  <th>Author</th>
                  <th>Category</th>
                  <th>Available</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {books.map((book) => (
                  <tr key={book.id}>
                    <td style={{ fontWeight: 500 }}>{book.title}</td>
                    <td><span className="badge badge-accent">{book.isbn}</span></td>
                    <td>{book.authorName}</td>
                    <td>{book.categoryName}</td>
                    <td>
                      <span className={`badge ${book.copiesAvailable > 0 ? 'badge-success' : 'badge-danger'}`}>
                        {book.copiesAvailable} / {book.totalCopies}
                      </span>
                    </td>
                    <td>
                      <div className="action-btns">
                        <button className="btn btn-ghost btn-sm" onClick={() => openEditModal(book)} title="Edit">
                          <Pencil size={14} />
                        </button>
                        <button className="btn btn-ghost btn-sm" onClick={() => handleDelete(book)} title="Delete" style={{ color: 'var(--color-danger)' }}>
                          <Trash2 size={14} />
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
            <Pagination page={page} totalPages={totalPages} totalCount={totalCount} pageSize={pageSize} onPageChange={setPage} />
          </>
        ) : (
          <EmptyState icon={BookOpen} title="No books found" message="Add your first book or adjust your search filters." />
        )}
      </div>

      {/* Create / Edit Modal */}
      <Modal
        isOpen={showModal}
        onClose={() => setShowModal(false)}
        title={editingBook ? 'Edit Book' : 'Add New Book'}
        footer={
          <>
            <button className="btn btn-ghost" onClick={() => setShowModal(false)}>Cancel</button>
            <button className="btn btn-primary" onClick={handleSubmit} disabled={saving}>
              {saving ? 'Saving...' : (editingBook ? 'Update' : 'Create')}
            </button>
          </>
        }
      >
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label className="form-label">Title *</label>
            <input className="form-input" name="title" value={form.title} onChange={handleChange} required />
          </div>
          {!editingBook && (
            <div className="form-group">
              <label className="form-label">ISBN *</label>
              <input className="form-input" name="isbn" value={form.isbn} onChange={handleChange} required maxLength={13} />
            </div>
          )}
          <div className="form-group">
            <label className="form-label">Description</label>
            <textarea className="form-textarea" name="description" value={form.description} onChange={handleChange} />
          </div>
          <div className="form-row">
            <div className="form-group">
              <label className="form-label">Author *</label>
              <select className="form-select" name="authorId" value={form.authorId} onChange={handleChange} required>
                <option value="">Select Author</option>
                {authors.map(a => <option key={a.id} value={a.id}>{a.fullName}</option>)}
              </select>
            </div>
            <div className="form-group">
              <label className="form-label">Category *</label>
              <select className="form-select" name="categoryId" value={form.categoryId} onChange={handleChange} required>
                <option value="">Select Category</option>
                {categories.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
              </select>
            </div>
          </div>
          <div className="form-row">
            {!editingBook && (
              <div className="form-group">
                <label className="form-label">Published Date</label>
                <input className="form-input" type="date" name="publishedDate" value={form.publishedDate} onChange={handleChange} />
              </div>
            )}
            <div className="form-group">
              <label className="form-label">Total Copies *</label>
              <input className="form-input" type="number" name="totalCopies" value={form.totalCopies} onChange={handleChange} min={1} required />
            </div>
            {editingBook && (
              <div className="form-group">
                <label className="form-label">Available Copies</label>
                <input className="form-input" type="number" name="copiesAvailable" value={form.copiesAvailable} onChange={handleChange} min={0} />
              </div>
            )}
          </div>
        </form>
      </Modal>
    </div>
  );
}

export default BooksPage;
