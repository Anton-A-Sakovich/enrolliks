const createPersonResultType = exports.createPersonResultType = {
    success: 0,
    conflict: 1,
    validationFailure: 2,
    badRequest: 3,
    serverError: 4,
    unknownError: 5,
};

exports.createPerson = async (post, personToCreate) => {
    const response = await post('/api/people/create', personToCreate);

    if (response.status == 201) {
        return {
            tag: createPersonResultType.success,
            createdPerson: response.data,
        };
    }

    if (response.status == 400) {
        if (response.isHttpProblem) {
            return {
                tag: createPersonResultType.badRequest,
            };
        } else {
            return {
                tag: createPersonResultType.validationFailure,
                validationErrors: response.data,
            };
        }
    }

    if (response.status == 409) {
        return {
            tag: createPersonResultType.conflict,
        };
    }

    if (response.status == 500) {
        return {
            tag: createPersonResultType.serverError,
        };
    }

    return {
        tag: createPersonResultType.unknownError,
    };
}

const deletePersonResultType = exports.deletePersonResultType = {
    success: 0,
    notFound: 1,
    badRequest: 2,
    serverError: 3,
    unknownError: 4,
};

exports.deletePerson = async (del, name) => {
    const response = await del('/api/people/delete', name);

    switch (response.status) {
        case 204:
            return {
                tag: deletePersonResultType.success,
            };
        
        case 404:
            return {
                tag: deletePersonResultType.notFound,
            };

        case 400:
            return {
                tag: deletePersonResultType.badRequest,
            };

        case 500:
            return {
                tag: deletePersonResultType.serverError,
            };

        default:
            return {
                tag: deletePersonResultType.unknownError,
            };
    }
};