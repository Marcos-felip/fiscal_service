using FiscalService.Application.Mappers;
using FiscalService.Application.Validators;
using FiscalService.Domain.Eventos;
using FluentValidation;

namespace FiscalService.Application.UseCases.CartaCorrecao;

public class CartaCorrecaoValidator : AbstractValidator<CartaCorrecaoRequest>
{
    public CartaCorrecaoValidator()
    {
        RuleFor(x => x.ChaveAcesso)
            .NotEmpty().WithMessage("Chave de acesso e obrigatoria")
            .Must(ValidacoesFiscais.ChaveAcessoValida)
            .WithMessage("Chave de acesso invalida");

        RuleFor(x => x.Correcao)
            .NotEmpty().WithMessage("Texto da correcao e obrigatorio")
            .MinimumLength(RegrasDeEvento.CorrecaoTamanhoMinimo)
            .WithMessage(
                $"Texto da correcao deve ter no minimo {RegrasDeEvento.CorrecaoTamanhoMinimo} caracteres")
            .MaximumLength(RegrasDeEvento.CorrecaoTamanhoMaximo)
            .WithMessage(
                $"Texto da correcao deve ter no maximo {RegrasDeEvento.CorrecaoTamanhoMaximo} caracteres");

        // Faixa legal apenas. Repeticao e salto dependem do historico da nota, e
        // o motor nao persiste nada — quem confere e o chamador.
        RuleFor(x => x.SequenciaEvento)
            .InclusiveBetween(
                RegrasDeEvento.SequenciaMinima,
                RegrasDeEvento.SequenciaMaxima)
            .WithMessage(
                $"Sequencia do evento deve estar entre {RegrasDeEvento.SequenciaMinima} e " +
                $"{RegrasDeEvento.SequenciaMaxima} — o layout admite no maximo " +
                $"{RegrasDeEvento.SequenciaMaxima} cartas de correcao por nota");

        RuleFor(x => x.CpfCnpj)
            .NotEmpty().WithMessage("CNPJ ou CPF do autor do evento e obrigatorio")
            .Must(ValidacoesFiscais.CpfOuCnpjValido)
            .WithMessage("CNPJ ou CPF do autor do evento e invalido");

        RuleFor(x => x.CertificadoBase64)
            .NotEmpty().WithMessage("Certificado e obrigatorio");

        RuleFor(x => x.CertificadoSenha)
            .NotEmpty().WithMessage("Senha do certificado e obrigatoria");

        RuleFor(x => x.Ambiente)
            .NotEmpty().WithMessage("Ambiente e obrigatorio")
            .Must(amb => NfceMapper.TryToAmbiente(amb, out _))
            .WithMessage("Ambiente invalido. Use 'producao' ou 'homologacao'");
    }
}
