const React = require('react');
const CreatePersonExpander = require('./createPersonExpander');
const PersonRow = require('./personRow');

const getAllPeopleResultType = {
    success: 'Success',
    failure: 'RepositoryFailure',
};

module.exports = class PeoplePage extends React.Component {
    render() {
        const args = this.props.args;

        let content = <p>Loading...</p>;
        if (args === null || !('tag' in args) || args.tag !== getAllPeopleResultType.success) {
            content = <p>Error.</p>;
        } else {
            const people = args.value.people;
            if (people.length === 0) {
                content = <p>No people found.</p>;
            }
            else {
                content = <PeopleTable people={people} />;
            }
        }

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
                    {this.props.people.map(person => <PersonRow key={person.name} person={person} />)}
                </tbody>
            </table>
        );
    }
}