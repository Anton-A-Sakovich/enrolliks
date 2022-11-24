const React = require('react');
const { createRoot } = require('react-dom/client');
const { createBrowserRouter, RouterProvider } = require('react-router-dom');

const Root = require('./root');
const HomePageWrapper = React.lazy(() => import('./home/homePage'));
const PeoplePageWrapper = React.lazy(() => import('./people/peoplePage'));

require('./main.css');

const router = createBrowserRouter([
    {
        path: '/',
        element: <Root />,
        children: [
            {
                index: true,
                element: (<React.Suspense fallback={<p>Loading...</p>}>
                    <HomePageWrapper />
                </React.Suspense>),
            },
            {
                path: 'home',
                element: (<React.Suspense fallback={<p>Loading...</p>}>
                    <HomePageWrapper />
                </React.Suspense>),
            },
            {
                path: 'people',
                element: (<React.Suspense fallback={<p>Loading...</p>}>
                    <PeoplePageWrapper />
                </React.Suspense>),
            },
        ],
    },
]);

const rootDiv = document.createElement('div');
document.body.appendChild(rootDiv);
createRoot(rootDiv).render(
    <React.StrictMode>
        <RouterProvider router={router} />
    </React.StrictMode>
);