const React = require('react');
const CreatePersonExpander = require('./createPersonExpander');
const PersonRow = require('./personRow');

const getAllPeopleResultType = {
    success: 'Success',
    failure: 'RepositoryFailure',
};

module.exports = class PeoplePage extends React.Component {
    constructor(props) {
        super(props);

        const args = this.props.args;
        if (args === null || !('tag' in args) || args.tag !== getAllPeopleResultType.success) {
            this.state = {
                people: [],
                isError: true,
            };
        } else {
            this.state = {
                people: args.value.people,
                isError: false,
            };
        }
    }

    render() {
        const content = this.state.isError
            ? <p>Error.</p>
            : this.state.people.length === 0
                ? <p>No people found.</p>
                : <PeopleTable
                    people={this.state.people}
                    onPersonRemoved={this.handlePersonRemoved} />;

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

    handlePersonRemoved = index => {
        this.setState(previousState => {
            const peopleCopy = previousState.people.slice();
            peopleCopy.splice(index, 1)
            
            return {
                people: peopleCopy,
            };
        });
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