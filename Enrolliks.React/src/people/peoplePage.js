const React = require('react');
const CreatePersonExpander = require('./createPersonExpander');
const PersonRow = require('./personRow');
const { getPeopleResultType } = require('./personEndpoints');
const { useLoaderData } = require('react-router-dom');
const { useState, useEffect } = require('react');

module.exports = function PeoplePage() {
    const getPeopleResult = useLoaderData();

    const [isError, setIsError] = useState(true);
    const [people, setPeople] = useState([]);

    useEffect(() => {
        if (getPeopleResult.tag === getPeopleResultType.success) {
            setIsError(false);
            setPeople(getPeopleResult.people);
        } else {
            setIsError(true);
            setPeople([]);
        }
    }, [getPeopleResult]);

    const handlePersonRemoved = index => {
        setPeople(previousPeople => {
            const peopleCopy = previousPeople.slice();
            peopleCopy.splice(index, 1)
            return peopleCopy;
        });
    }

    const content = isError
            ? <p>Error.</p>
            : people.length === 0
                ? <p>No people found.</p>
                : <PeopleTable
                    people={people}
                    onPersonRemoved={handlePersonRemoved} />;

    return (
        <div>
            <div>
                <CreatePersonExpander />
            </div>
            <div className='panel'>
                {content}
            </div>
        </div>
    );
}

class PeopleTable extends React.Component {
    render() {
        return (
            <table>
                <thead>
                    <tr>
                        <th>Name</th>
                    </tr>
                </thead>
                <tbody>
                    {this.props.people.map((person, index) =>
                        <PersonRow
                            key={person.name}
                            person={person}
                            onRemoved={() => this.props.onPersonRemoved(index)} />
                    )}
                </tbody>
            </table>
        );
    }
}