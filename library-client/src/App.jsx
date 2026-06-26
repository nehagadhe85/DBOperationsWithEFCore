import React, { lazy, Suspense } from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { Toaster } from 'react-hot-toast';
import Layout from './components/Layout';
import { Loading } from './components/StatusComponents';

// React.lazy — Code splitting: each page loads only when navigated to
const Dashboard = lazy(() => import('./pages/Dashboard'));
const BooksPage = lazy(() => import('./pages/BooksPage'));
const AuthorsPage = lazy(() => import('./pages/AuthorsPage'));
const CategoriesPage = lazy(() => import('./pages/CategoriesPage'));
const MembersPage = lazy(() => import('./pages/MembersPage'));
const LoansPage = lazy(() => import('./pages/LoansPage'));

function App() {
  return (
    <Router>
      <Toaster
        position="top-right"
        toastOptions={{
          duration: 3000,
          style: {
            background: '#1e1d36',
            color: '#fffffe',
            border: '1px solid rgba(255,255,255,0.08)',
            fontSize: '14px',
            fontFamily: "'Inter', sans-serif",
          },
          success: { iconTheme: { primary: '#2cb67d', secondary: '#fff' } },
          error: { iconTheme: { primary: '#e53170', secondary: '#fff' } },
        }}
      />
      <Layout>
        <Suspense fallback={<Loading />}>
          <Routes>
            <Route path="/" element={<Dashboard />} />
            <Route path="/books" element={<BooksPage />} />
            <Route path="/authors" element={<AuthorsPage />} />
            <Route path="/categories" element={<CategoriesPage />} />
            <Route path="/members" element={<MembersPage />} />
            <Route path="/loans" element={<LoansPage />} />
          </Routes>
        </Suspense>
      </Layout>
    </Router>
  );
}

export default App;
