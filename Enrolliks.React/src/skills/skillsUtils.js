const { isObject, hasProperty, isString, and, or } = require('../predicates');

module.exports.isSkill = and(isObject,
    hasProperty('id', isString),
    hasProperty('name', isString));

module.exports.isSkillValidationErrors = and(isObject, or(
    hasProperty('id', isObject),
    hasProperty('name', isObject)
));