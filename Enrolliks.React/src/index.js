const React = require('react');
const { createRoot } = require('react-dom/client');
const { createBrowserRouter, RouterProvider } = require('react-router-dom');

const Root = require('./root');
const HomePageWrapper = React.lazy(() => import('./home/homePage'));
const PeoplePageWrapper = React.lazy(() => import('./people/peoplePage'));
const peoplePageLoader = () => import('./people/peopleLoader').then(({default: loader}) => loader());

require('./main.css');

function suspensed(element) {
    const fallback = <p>Loading...</p>;
    return (<React.Suspense fallback={fallback}>
        {element}
    </React.Suspense>);
}

const router = createBrowserRouter([
    {
        path: '/',
        element: <Root />,
        children: [
            {
                index: true,
                element: suspensed(<HomePageWrapper />),
            },
            {
                path: 'home',
                element: suspensed(<HomePageWrapper />),
            },
            {
                path: 'people',
                loader: peoplePageLoader,
                element: suspensed(<PeoplePageWrapper />),
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